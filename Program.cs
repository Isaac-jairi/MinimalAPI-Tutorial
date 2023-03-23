using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniValidation;
using NetDevPack.Identity.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/fornecedor", async (MinimalContextDb context) => {
    return await context.Fornecedores.ToListAsync();
});

app.MapGet("/fornecedor/{id}", async (MinimalContextDb context, Guid id) =>
{
    var fornecedor = await context.Fornecedores.FindAsync(id);
    if (fornecedor is null)
        return Results.NotFound("Não foi encontrado usuario com esse ID");
    return Results.Ok(fornecedor);
})
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");

app.MapPost("/fornecedor", async (MinimalContextDb context, Fornecedor fornecedor) =>
{
    if (MiniValidator.TryValidate(fornecedor, out var errors))
        Results.ValidationProblem(errors);

    context.Fornecedores.Add(fornecedor);

    var result = await context.SaveChangesAsync();
    return result > 0
        ? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor)
        : Results.BadRequest("Houve um problema ao salvar o registro");
})
    .ProducesValidationProblem()
    .Produces<Fornecedor[]>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostFornecedor")
    .WithTags("Fornecedor");

app.MapPut("/fornecedor", async (MinimalContextDb context, Fornecedor fornecedor) =>
{
    if (MiniValidator.TryValidate(fornecedor, out var errors))
        Results.ValidationProblem(errors);

    var fornecedorFound = await context.Fornecedores.FindAsync(fornecedor.Id);
    if (fornecedorFound is null)
        return Results.NotFound("Não foi encontrado usuario com esse ID");

    fornecedorFound.Nome = fornecedor.Nome;
    fornecedorFound.Documento = fornecedor.Documento;
    fornecedorFound.Ativo = fornecedor.Ativo;

    var result = await context.SaveChangesAsync();
    return result > 0
        ? Results.Ok(fornecedor)
        : Results.BadRequest("Houve um problema ao atualizar o registro");
})
    .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");


app.MapDelete("/fornecedor/{id}", async (MinimalContextDb context, Guid id) =>
{
    var fornecedorFound = await context.Fornecedores.FindAsync(id);
    if (fornecedorFound is null)
        return Results.NotFound("Não foi encontrado usuario com esse ID");

    context.Fornecedores.Remove(fornecedorFound);

    var result = await context.SaveChangesAsync();
    return result > 0
        ? Results.Ok("Usuário deletado")
        : Results.BadRequest("Houve um problema ao deletar o registro");
})
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("DeleteFornecedor")
    .WithTags("Fornecedor");
app.Run();