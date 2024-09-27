#!/usr/bin/env cargo-eval
//! Dependencies can be specified in the script file itself as follows:
//!
//! ```cargo
//! [dependencies]
//! anyhow = { version = "1.0.75", features = ["backtrace"] }
//! heck = "0.5.0"
//! regex = "1.10.2"
//! prost = { version = "0.13.3", features = ["prost-derive"] }
//! prost-build = "0.13.3"
//! ```

use anyhow::{Ok, Result};
use std::fs;
use std::path::Path;

pub fn walk_dir(
    root_path: &Path,
    extensions: &Vec<&str>,
    relative_path_str: &str,
) -> Result<Vec<String>> {
    let mut files: Vec<String> = vec![];
    let dir = root_path.join(relative_path_str);
    let dir_content = fs::read_dir(dir)
        .unwrap()
        .map(|e| e.unwrap().path())
        .collect::<Vec<_>>();

    for entry_path in dir_content {
        let metadata = fs::metadata(&entry_path).unwrap();
        if metadata.is_dir() {
            walk_dir(root_path, extensions, entry_path.to_str().unwrap())?
                .iter()
                .for_each(|f| files.push(f.to_string()));
        } else {
            let mut should_add = false;

            if extensions.len() <= 0 {
                should_add = true;
            } else {
                // println!("entry_path: {:?}", entry_path);
                let ext = Path::extension(entry_path.as_path());
                if ext.is_none() {
                    continue;
                }

                let ext = ext.unwrap().to_str().unwrap();
                if extensions.contains(&ext) {
                    should_add = true;
                }
            }

            if should_add {
                files.push(String::from(
                    entry_path
                        .strip_prefix(root_path.to_str().unwrap())
                        .unwrap()
                        .to_str()
                        .unwrap(),
                ));
            }
        }
    }
    Ok(files)
}

fn main() {
    let arg_protos = std::env::args().nth(1).expect("No protos path provided");
    let arg_des = std::env::args().nth(2).expect("No des path provided");
    let protos_path = Path::new(&arg_protos);

    let files = walk_dir(protos_path, &vec!["proto"], "")
        .unwrap()
        .iter()
        .map(|f| protos_path.join(f).to_string_lossy().to_string())
        .collect::<Vec<_>>();

    println!("files: {:?}", files);

    std::env::set_var("OUT_DIR", &arg_des);
    prost_build::compile_protos(&files, &[&protos_path.to_string_lossy().to_string()]).unwrap();

    let des_path = Path::new(&arg_des);
    let files = walk_dir(des_path, &vec!["rs"], "").unwrap();

    println!("files: {:?}", files);

    let mut lib = String::new();
    lib.push_str("pub mod proto {\n");
    for file in files {
        lib.push_str(&format!(
            "    pub mod {};\n",
            file.replace("/", "_").replace(".rs", "")
        ));
    }
    lib.push_str("}\n");
    let lib_path = des_path.parent().unwrap().join("lib.rs");
    fs::write(lib_path, lib).unwrap();
}
