# Protocol Buffers for Unity

this is a simple example of using Protocol Buffers http post between Unity Client and Rust Server

## Library Versions

### Client Unity Csharp

-   Protocol Buffers Version
    <https://github.com/protocolbuffers/protobuf/tree/v28.2>
-   System.Runtime.CompilerServices.Unsafe Version
    <https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0>

### Server Rust

-   Prost
    <https://crates.io/crates/prost>
-   Actix
    <https://crates.io/crates/actix-web>

## Usage

-   place proto files in `proto/protos` directory

-   generate protobuf files

    ```shell
    sh proto/build-proto.sh
    ```

-   start server

    ```shell
    cd server-rs
    cargo run
    ```

-   start client

    open Unity project and run
