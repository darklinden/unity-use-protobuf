#!/usr/bin/env bash

# https://github.com/fornwall/rust-script
# cargo install rust-script

SOURCE=${BASH_SOURCE[0]}
while [ -L "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
    DIR=$(cd -P "$(dirname "$SOURCE")" >/dev/null 2>&1 && pwd)
    SOURCE=$(readlink "$SOURCE")
    [[ $SOURCE != /* ]] && SOURCE=$DIR/$SOURCE # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
SCRIPT_DIR=$(cd -P "$(dirname "$SOURCE")" >/dev/null 2>&1 && pwd)

echo "Build protos for Unity"
PROTO_DIR="$(realpath "${SCRIPT_DIR}/protos")"
echo "PROTO_DIR: ${PROTO_DIR}"

UNITY_PROTO_DIR="$(realpath "${SCRIPT_DIR}/../unity/Assets/Plugins/Proto")"
echo "UNITY_PROTO_DIR: ${UNITY_PROTO_DIR}"

# Check if the proto directory exists
rm -rf "${UNITY_PROTO_DIR}"
if [ ! -d "${UNITY_PROTO_DIR}" ]; then
    mkdir -p "${UNITY_PROTO_DIR}"
fi

# Generate the protobuf files
protoc -I=$PROTO_DIR --csharp_out=$UNITY_PROTO_DIR $PROTO_DIR/*.proto

if [ $? -ne 0 ]; then
    echo "Failed to Build protos for Unity"
    exit 1
fi

echo "Build protos for Rust"

cd "${SCRIPT_DIR}" || exit 1

RUST_PROTO_DIR="$(realpath "${SCRIPT_DIR}/../server-rs/protocols/src/proto")"
echo "RUST_PROTO_DIR: ${RUST_PROTO_DIR}"

# Check if the proto directory exists
rm -rf "${RUST_PROTO_DIR}"
if [ ! -d "${RUST_PROTO_DIR}" ]; then
    mkdir -p "${RUST_PROTO_DIR}"
fi

rust-script --cargo-output --debug proto-build.rs "${PROTO_DIR}" "${RUST_PROTO_DIR}"
