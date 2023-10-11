using Microsoft.AspNetCore.Mvc;
using UnitTestingA1Base.Models;
using UnitTestingA1Base.Models.ViewModels;

namespace UnitTestingA1Base.Data
{
    public class BusinessLogicLayer
    {
        private AppStorage _appStorage;

        public BusinessLogicLayer(AppStorage appStorage) {
            _appStorage = appStorage;
        }

        /// Returns a HashSet of all Recipes that contain the specified Ingredient by name or Primary Key
        public HashSet<Recipe> GetRecipesByIngredient(int? id, string? name)
        {
            Ingredient ingredient = null;
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                ingredient = _appStorage.Ingredients.FirstOrDefault (i => i.Id == id);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                ingredient = _appStorage.Ingredients.FirstOrDefault (i => i.Name == name);
            }

            // If the ingredient is found, find recipes containing it.
            if (ingredient != null)
            {

                HashSet<RecipeIngredient> recipeIngredients = _appStorage.RecipeIngredients.Where(rI => rI.IngredientId == ingredient.Id).ToHashSet();

                recipes = _appStorage.Recipes.Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id)).ToHashSet();
            }
            else 
            {
                return null;
            }

            return recipes;
        }

        /// Returns a HashSet of all Recipes that only contain ingredients that belong to the Dietary Restriction provided by name or Primary Key
        public HashSet<Recipe> GetRecipesByDietaryRestriction(int? id, string? name)
        {
            DietaryRestriction dietaryRestriction = null;
            IngredientRestriction ingredientRestriction = null;
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                dietaryRestriction = _appStorage.DietaryRestrictions.FirstOrDefault(i => i.Id == id);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                dietaryRestriction = _appStorage.DietaryRestrictions.FirstOrDefault(i => i.Name == name);
            }

            // If the dietaryRestriction is found, find recipes containing it.
            if (dietaryRestriction != null)
            {
                ingredientRestriction = _appStorage.IngredientRestrictions.FirstOrDefault(i => i.DietaryRestrictionId == dietaryRestriction.Id);

                if (ingredientRestriction != null)
                {

                    HashSet<RecipeIngredient> recipeIngredients = _appStorage.RecipeIngredients.Where(rI => rI.IngredientId == ingredientRestriction.IngredientId).ToHashSet();

                    recipes = _appStorage.Recipes.Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id)).ToHashSet();
                }
            }
            else
            {
                return null;
            }

            return recipes;
        }

        ///Returns a HashSet of all recipes by either Name or Primary Key. 
        public HashSet<Recipe> GetRecipes(int? id, string? name)
        {
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                Recipe recipe = _appStorage.Recipes.FirstOrDefault(i => i.Id == id); //_appStorage.Recipes.Where(r => r.Id == id).ToHashSet();

                if (recipe == null)
                {
                    return null;
                }

                recipes.Add(recipe);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                Recipe recipe = _appStorage.Recipes.FirstOrDefault(i => i.Name == name);

                if (recipe == null)
                {
                    return null;
                }

                recipes.Add(recipe);
            }
            else 
            {
                recipes = _appStorage.Recipes.ToHashSet();
            }

            return recipes;
        }

        /// If a Recipe with the same name as the new Recipe already exists, an InvalidOperation exception should be thrown.
        /// If an Ingredient with the same name as an existing Ingredient is provided, it should not add that ingredient to storage.
        /// The method should add all Recipes and (new) ingredients to the database. It should create RecipeIngredient objects and add them to the database to represent their relationship. Remember to use the IDs of any preexisting ingredients rather than new IDs.
        /// All IDs should be created for these objects using the returned value of the AppStorage.GeneratePrimaryKey() method
        public void AddRecipes(HashSet<RecipeWithIngredientsViewModel> recipesWithIngredients)
        {
            if (recipesWithIngredients == null)
            {
                throw new ArgumentNullException(nameof(recipesWithIngredients));
            }

            foreach (RecipeWithIngredientsViewModel recipeWithIngredients in recipesWithIngredients)
            {
                // Check if a Recipe with the same name already exists.
                if (_appStorage.Recipes.Any(r => r.Name == recipeWithIngredients.Recipe.Name))
                {
                    throw new InvalidOperationException("A Recipe with the same name already exists.");
                }

                // Create a new Recipe with a new ID.
                Recipe newRecipe = new Recipe
                {
                    Id = _appStorage.GeneratePrimaryKey(),
                    Name = recipeWithIngredients.Recipe.Name,
                    
                };

                // Add the new Recipe to the Storage.
                _appStorage.Recipes.Add(newRecipe);

                // Create RecipeIngredient objects and add them to the database.
                foreach (Ingredient ingredient in recipeWithIngredients.Ingredients)
                {
                    // Check if an Ingredient with the same name already exists.
                    Ingredient existingIngredient = _appStorage.Ingredients.FirstOrDefault(i => i.Name == ingredient.Name);

                    // If it exists, use the existing Ingredient's ID.
                    if (existingIngredient != null)
                    {
                        _appStorage.RecipeIngredients.Add(new RecipeIngredient
                        {
                            RecipeId = newRecipe.Id,
                            IngredientId = existingIngredient.Id
                        });
                    }
                    else
                    {
                        // Create a new Ingredient with a new ID.
                        Ingredient newIngredient = new Ingredient
                        {
                            Id = _appStorage.GeneratePrimaryKey(),
                            Name = ingredient.Name,
                            
                        };

                        // Add the new Ingredient to the database.
                        _appStorage.Ingredients.Add(newIngredient);

                        // Add a new RecipeIngredient for the new Ingredient.
                        _appStorage.RecipeIngredients.Add(new RecipeIngredient
                        {
                            RecipeId = newRecipe.Id,
                            IngredientId = newIngredient.Id
                        });
                    }
                }
            }
        }


        /// Deletes an ingredient from the database. 
        /// If there is only one Recipe using that Ingredient, then the Recipe is also deleted, as well as all associated RecipeIngredients
        /// If there are multiple Recipes using that ingredient, a Forbidden response code should be provided with an appropriate message
        public IResult DeleteAnIngredient(int? id, string? name)
        {
            Ingredient ingredientToDelete = null;

            if (id != 0)
            {
                ingredientToDelete = _appStorage.Ingredients.FirstOrDefault(i => i.Id == id);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                ingredientToDelete = _appStorage.Ingredients.FirstOrDefault(i => i.Name == name);
            }

            if (ingredientToDelete == null)
            {
                // Ingredient not found, return a Not Found response.
                return Results.NotFound();//Result("Ingredient not found.");
            }

            // Check how many recipes use the ingredient.
            int recipeCount = _appStorage.RecipeIngredients.Count(ri => ri.IngredientId == ingredientToDelete.Id);

            if (recipeCount == 0)
            {
                // No recipes use this ingredient, so delete it.
                _appStorage.Ingredients.Remove(ingredientToDelete);
                return Results.NoContent(); // Successful deletion with no content.
            }
            else if (recipeCount == 1)
            {
                // Only one recipe uses this ingredient, delete it and the associated recipe.
                Recipe recipeToDelete = _appStorage.Recipes.First(r => r.Id == _appStorage.RecipeIngredients.First(ri => ri.IngredientId == ingredientToDelete.Id).RecipeId);
                _appStorage.RecipeIngredients.RemoveWhere(ri => ri.IngredientId == ingredientToDelete.Id);
                _appStorage.Recipes.Remove(recipeToDelete);
                _appStorage.Ingredients.Remove(ingredientToDelete);
                return Results.NoContent(); // Successful deletion with no content.
            }
            else
            {
                // Multiple recipes use this ingredient, return a Forbidden response.
                // return Results.StatusCode(403);//("Cannot delete ingredient as it is used in multiple recipes.");
                return Results.Forbid();// ("Cannot delete ingredient as it is used in multiple recipes.");
            }
        }

    }
}
