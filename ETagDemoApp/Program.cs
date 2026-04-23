using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AutomatedETags;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add library
builder.Services.AddAutomatedETags(options =>
{
    options.Mode = ETagMode.OptIn;              
    options.Algorithm = ETagAlgorithm.XxHash64; 
    options.MaxBodySize = 1048576; // 1mb
    options.UseWeakETags = false;              
});

var app = builder.Build();

app.UseRouting();

// Turn on middleware
app.UseAutomatedETags();

app.MapControllers();
app.Run();