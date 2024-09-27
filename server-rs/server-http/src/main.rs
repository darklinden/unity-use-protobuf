use actix_protobuf::{ProtoBuf, ProtoBufResponseBuilder as _};
use actix_web::{middleware, web, App, HttpResponse, HttpServer, Result};
use protocols::proto::tutorial::Person;

async fn index(msg: ProtoBuf<Person>) -> Result<HttpResponse> {
    println!("model: {msg:?}");
    HttpResponse::Ok().protobuf(msg.0) // <- send response
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    println!("starting HTTP server at http://localhost:3000");

    HttpServer::new(|| {
        App::new()
            .service(web::resource("/").route(web::post().to(index)))
            .wrap(middleware::Logger::default())
    })
    .workers(1)
    .bind(("0.0.0.0", 3000))?
    .run()
    .await
}
