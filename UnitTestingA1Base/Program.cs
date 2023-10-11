#region Setup
using System.Xml.Linq;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;
using UnitTestingA1Base.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

// Application Storage persists for single session
AppStorage appStorage = new AppStorage();
BusinessLogicLayer bll = new BusinessLogicLayer(appStorage);    
#endregion


#region Endpoints

/*
 * All methods should return a NotFound response code if a search using a Primary Key does not find any results.
 * 
 * Otherwise, the method should return an empty collection of the resource indicated in the root directory of the request.
 * 
 * All methods with a name and id parameter can use either. 
 * 
 * Searches by name should use the first queried element that contains the entire string (for example, "spa" returns "spaghetti" while "spaghettio" returns nothing).
 * 
 * Requests with a Name AND ID argument should query the database using the ID. If no match is found, the query should again be performed using the name.
 */

///<summary>
/// Returns a HashSet of all Recipes that contain the specified Ingredient by name or Primary Key
/// </summary>
app.MapGet("/recipes/byIngredient", (string? name, int? id) =>
{
    try 
    {
        HashSet<Recipe> recipes = bll.GetRecipesByIngredient(id, name);
        return Results.Ok(recipes);
    } catch(Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
/// Returns a HashSet of all Recipes that only contain ingredients that belong to the Dietary Restriction provided by name or Primary Key
/// </summary>
app.MapGet("/recipes/byDiet", (string? name, int? id) =>
{
    try
    {
        HashSet<Recipe> recipes = bll.GetRecipesByDietaryRestriction(id, name);
        return Results.Ok(recipes);
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
///Returns a HashSet of all recipes by either Name or Primary Key. 
/// </summary>
app.MapGet("/recipes", (string? name, int? id) =>
{
    try
    {
        HashSet<Recipe> recipes = bll.GetRecipes(id, name);
        return Results.Ok(recipes);
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
/// Receives a JSON object which should contain a Recipe and Ingredients
/// 
/// A new Class should be created to serve as the Parameter of this method
/// 
/// If a Recipe with the same name as the new Recipe already exists, an InvalidOperation exception should be thrown.
/// 
/// If an Ingredient with the same name as an existing Ingredient is provided, it should not add that ingredient to storage.
/// 
/// The method should add all Recipes and (new) ingredients to the database. It should create RecipeIngredient objects and add them to the database to represent their relationship. Remember to use the IDs of any preexisting ingredients rather than new IDs.
/// 
/// All IDs should be created for these objects using the returned value of the AppStorage.GeneratePrimaryKey() method
/// </summary>
app.MapPost("/recipes", (HashSet<RecipeWithIngredientsViewModel> recipesWithIngredients) => {
    try
    {
        bll.AddRecipes(recipesWithIngredients);
 
        return Results.Ok();
    }
    catch (Exception ex)
    {
       return Results.BadRequest();
    }

});

///<summary>
/// Deletes an ingredient from the database. 
/// If there is only one Recipe using that Ingredient, then the Recipe is also deleted, as well as all associated RecipeIngredients
/// If there are multiple Recipes using that ingredient, a Forbidden response code should be provided with an appropriate message
///</summary>
app.MapDelete("/ingredients", (int? id, string? name) =>
{
    try
    {
        bll.DeleteAnIngredient(id, name);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

/// <summary>
/// Deletes the requested recipe from the database
/// This should also delete the associated IngredientRecipe objects from the database
/// </summary>
app.MapDelete("/recipes", (int id, string name) =>
{

});

#endregion


app.Run();
