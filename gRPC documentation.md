# Documentation gRPC (Google Remote Procedure Call) 

## Documentation Officielle Windows : 

https://learn.microsoft.com/fr-fr/aspnet/core/grpc/basics?view=aspnetcore-7.0 :white_check_mark: 

## Créer un service gRPC ASP.NET Core :

<img src="Capture d'écran 1.png" alt="image-20230912111502315" style="zoom:80%;" />

## Fichier Contrat : 

Dans l'environnement .NET Core, on définit le contrat de service et les types de messages en utilisant les fichiers `.proto` .

Voici un exemple de fichier `.proto` :

```c#
syntax = "proto3";

service MyService {
    rpc MyMethod1 (MyRequest) returns (MyResponse);
    rpc MyMethod2 (MyRequest) returns (MyResponse);
}

message MyRequest {
    string id = 1;
    string name = 2;
}

message MyResponse {
    int32 age = 1;
    string body = 2;
}

```

1. La première ligne est obligatoirement ```syntax = "proto3";```
2. La deuxième ligne sert à définir ce que fait notre service. En fait, on peut remplacer le mot `MyService` par `API`.
   - Dans le service on définit autant de méthodes qu'on veut. Ces méthodes correspondent aux traditionnelles 'Routes & Endpoints' en `REST`.
3. Une section `message` correspond à ce qu'on pourrait attendre d'un `JSON` en `REST`.
   - Par la suite, on pourra par exemple accéder à **`MyRequest.Id`**, ou **`MyResponse.Body`** *(notez la majuscule à `Id` et `Body`)*.



:pushpin: Voici la <u>documentation officielle</u> pour écrire un fichier `.proto` . *https://protobuf.dev/programming-guides/proto3/*







