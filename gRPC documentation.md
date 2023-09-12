# Documentation gRPC (Google Remote Procedure Call) 



## Lien vers la Documentation Officielle Windows, cliquer [ici](https://learn.microsoft.com/fr-fr/aspnet/core/grpc/basics?view=aspnetcore-7.0) :round_pushpin:



## Créer un service gRPC ASP.NET Core :

<img src="Capture d'écran 1.png" alt="image-20230912111502315" style="zoom:80%;" />





## Fichier Contrat : 

Dans l'environnement .NET Core, on définit le contrat de service et les types de messages en utilisant les fichiers `.proto` [^1].

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

[^1]: :pushpin: Voici la <u>documentation officielle</u> pour écrire un fichier `.proto` . *https://protobuf.dev/programming-guides/proto3/*







## Relier le client au serveur par le contrat (fichier `.proto`)

Avant de coder la partie client de votre application, il faut que celle-ci soit reliée au serveur ( ou inversement ).

Pour cela suivez ces étapes.

Positionnez-vous sur le projet client, puis faites `Ajouter` , puis  `Service Connecté`. 

<img src="Capture d'écran 2.png" alt="image-20230912111502316" style="zoom:100%;" />

<img src="Capture d'écran 3.png" alt="image-20230912111502317" style="zoom:100%;" />

Puis sélectionnez `gRPC` et cliquez sur `Suivant`.

Enfin sélectionnez le fichier `.proto` souhaité et cliquez sur `Terminer`.







## Dossier 'Services'

C'est ici où les appels à l'API doivent être implémentés.

<img src="Capture d'écran 4.png" alt="image-20230912111502315" style="zoom:100%;" />



Supposons qu'on ait ce fichier `greet.proto`: 

```c#
syntax = "proto3";

option csharp_namespace = "GrpcServer";

package greet;

// The greeting service definition.
service Greeter {
  // Sends a greeting
  rpc SayHello (HelloRequest) returns (HelloReply);
}

// The request message containing the user's name.
message HelloRequest {
  string name = 1;
}

// The response message containing the greetings.
message HelloReply {
  string message = 1;
}

```

Alors on peut implémenter la méthode `"Say Hello"` de cette manière dans le fichier `GreeterService.cs`

```c#
public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
{
    return Task.FromResult(new HelloReply
    {
        Message = "Hello " + request.Name
    });
}
```









## Authentification

Pour l'authentification, gRPC permet d'utilser les tokens JWT. 

Vous aurez besoin des packages suivants : 

<img src="Capture d'écran 5.png" alt="image-20230912111502315" style="zoom:100%;" />

En suppposant qu'on ait le fichier authentication.proto suivant : 

```c#
syntax = "proto3";

option csharp_namespace = "GrpcServer";

package authentication;

service Authentication {
  rpc Authenticate (AuthenticationRequest) returns (AuthenticationResponse);
}

message AuthenticationRequest{
	string UserName = 1;
	string Password = 2;
}

message AuthenticationResponse{
	string AccessToken = 1;
	int32 ExpiresIn = 2;
}
```



Voici un exemple de code pour Authentifier à l'aide d'un jeton JWT.

```c#
 public static AuthenticationResponse Authenticate(AuthenticationRequest authenticationRequest)
{
    // -- Implement User Credentials Validation
    var userRole = string.Empty;
    if(authenticationRequest.UserName == "admin" && authenticationRequest.Password == "admin")
    {
        userRole = "Administrator";
    }
     // else if UserName == 'user' && Password =='user'
     // userRole = "User" 
     // etc ...

    //------------------------------------
	// Create a JWT Security Token Handler: 
    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    // Configure JWT Token Settings:
    var tokenKey = Encoding.ASCII.GetBytes("WRITE YOUR KEY HERE");
    var tokenExpiryDateTime = DateTime.Now.AddMinutes(WRITE A NUMBER HERE);
    // Create a Security Token Descriptor:
    var securityTokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new List<Claim>
        {
            new Claim("username", authenticationRequest.UserName),
            new Claim(ClaimTypes.Role, userRole) //  -- Add User Role or change it
        }),
        Expires = tokenExpiryDateTime,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };
	// Create a JWT Token:
    var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
    // Write the JWT as a String:
    var token = jwtSecurityTokenHandler.WriteToken(securityToken);
	// Return the Authentication Response:
    return new AuthenticationResponse
    {
        AccessToken = token,
        ExpiresIn = (int)tokenExpiryDateTime.Subtract(DateTime.Now).TotalSeconds
    };
}
```



## Autorisations 

Les autorisations se gèrent de manière 'traditionnelles' en assignant des rôles aux utilisateurs.

Voyez l'existence des attributs personalisés d'autorisations.

- [AllowAnonymous]
- [Authorize(Roles = "MyRole")]

Un exemple de fichier qui n'autorise que certains utilisateurs à faire des opérations.

```c#
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace GrpcServer.Services
{
    public class CalculationService : Calculation.CalculationBase
    {
        [Authorize(Roles = "Administrator")]
        public override Task<CalculationResult> Add(InputNumbers request, ServerCallContext context)
        {
            return Task.FromResult(new CalculationResult { Result = request.Number1 + request.Number2 });
        }

        [Authorize(Roles = "Administrator,User")]
        public override Task<CalculationResult> Subtract(InputNumbers request, ServerCallContext context)
        {
            return Task.FromResult(new CalculationResult { Result = request.Number1 - request.Number2 });
        }

        [AllowAnonymous]
        public override Task<CalculationResult> Multiply(InputNumbers request, ServerCallContext context)
        {
            return Task.FromResult(new CalculationResult { Result = request.Number1 * request.Number2 });
        }
    }
}
```



:pushpin: Liens [tuto vidéo](https://www.youtube.com/watch?v=4-GTX6vW2Z4&t=2292s) et [repo github](https://github.com/codingdroplets/GrpcJwtAuthentication) sur l'authentification et les autorisations.



## Appels gRPC 

gRPC offre la possibilité d'échanger les informations du client et le serveur de 4 manières.

Prennons ce fichier `test.proto` : 

```c# 
service Greeter {
  rpc ServerStreaming (Test) returns (stream Test);
  rpc ClientStreaming(stream Test) returns (Test);
  rpc BidirectionalStreaming(stream Test) returns (stream Test);
}
message Test
{
	string TestMessage = 1;
}
```



### 1 - Echange Unaire 

Le serveur et le client s'échangent un par un les messages.

### 2 - Réponses par flux du serveur

Le serveur répond au client par un flux de messages.

```c#
public override async Task ServerStreaming(Test request, IServerStreamWriter<Test> responseStream, ServerCallContext context)
{
    for (int i = 0; i <= 10; i++)
    {
        await responseStream.WriteAsync(new Test { TestMessage = "i = " + i });
        await Task.Delay(1000);
    }
}
```



### 3 - Requêtes par flux du client

Le client envoit au serveur un flux de requêtes.

```c#
public static async Task ClientStreamingDemo()
{
    var channel = GrpcChannel.ForAddress("http://localhost:5219");
    var client = new Greeter.GreeterClient(channel);
    var response = client.ClientStreaming();
    for (int i = 0; i < 10; i++)
    {
        await response.RequestStream.WriteAsync(new Test { TestMessage = "numéro : " + i });
    }
    await response.RequestStream.CompleteAsync(); // signal the server stream's end (mandatory)
    Console.WriteLine(await response.ResponseAsync);
    await channel.ShutdownAsync();
}
```



### 4 - Communication Bi-Directionnelle par flux 

Le serveur répond au client par un flux de messages.

Le client envoit au serveur un flux de requêtes.

Voici un exemple de code , du côté serveur d'abord :

```c#
public override async Task BidirectionalStreaming(IAsyncStreamReader<Test> requestStream, IServerStreamWriter<Test> responseStream, ServerCallContext context)
{
    List<Task> tasks = new List<Task>();
    while( await requestStream.MoveNext(CancellationToken.None))
    {
        var message = requestStream.Current.TestMessage;
        Console.WriteLine("Received request = " + message );
        Task task = Task.Run(async () =>
        {
            await Task.Delay( new Random().Next(1,10) * 1000 ); // optionnal
            await responseStream.WriteAsync(new Test { TestMessage = "Server sent : " + message });
        }
        );
        tasks.Add(task);
    }
    await Task.WhenAll(tasks);
}
```

Dans cet exemple le serveur reçoit un message puis le renvoit au client.

Voici un exemple de code correspondant du côté client :

```c# 
static async Task BidirectionnalStreamingDemo()
{
    var channel = GrpcChannel.ForAddress("http://localhost:5219");
    var client = new Greeter.GreeterClient(channel);
    var stream = client.BidirectionalStreaming();
    
    
    // Send request to server
    var requestTask = Task.Run(async () =>
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(new Random().Next(1,10) * 1000 ); // optionnal
            await stream.RequestStream.WriteAsync(new Test { TestMessage = i.ToString() });
            Console.WriteLine("Nb request sent = " + i);
        }
        await stream.RequestStream.CompleteAsync();
    });
    
    
    // Get the responses from server
    var responseTask = Task.Run(async () =>
    {
        while (await stream.ResponseStream.MoveNext(CancellationToken.None))
        {
            Console.WriteLine("Response from server is : " + stream.ResponseStream.Current.TestMessage);
        }
    });
}
```

