using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;
using UnitTestingA1Base.Models.ViewModels;

namespace RecipeUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private BusinessLogicLayer _initializeBusinessLogic()
        {
            return new BusinessLogicLayer(new AppStorage());
        }

        [TestMethod]
        public void GetRecipesByIngredient_ValidId_ReturnsRecipesWithIngredient()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int ingredientId = 6;
            int recipeCount = 2;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(ingredientId, null);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByIngredient_ValidName_ReturnsRecipesWithIngredient()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            string ingredientName = "Eggs";
            int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(null, ingredientName);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByIngredient_InvalidId_ReturnsNull()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int ingredientId = 0; // invalidId

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(ingredientId, null);

            // assert
            Assert.IsNull(recipes);
        }


        [TestMethod]
        public void GetRecipesByDiet_ValidId_ReturnsRecipesWithIngredientBelongToDietaryRestriction()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int dietaryRestrictionId = 1;
            int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByDietaryRestriction(dietaryRestrictionId, null);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByDiet_ValidName_ReturnsRecipesWithIngredientBelongToDietaryRestriction()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            string dietaryRestrictionName = "Vegetarian";
            int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByDietaryRestriction(null, dietaryRestrictionName);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByDiet_InvalidId_ReturnsNull()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int dietaryRestrictionId = 0;
            // int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByDietaryRestriction(dietaryRestrictionId, null);

            // assert
            Assert.IsNull(recipes);
        }

        ///Returns a HashSet of all recipes by either Name or Primary Key. 
        [TestMethod]
        public void GetRecipes_ValidId_ReturnsRecipesById()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int recipeId = 1;
            int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipes(recipeId, null);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipes_ValidName_ReturnsRecipesByName()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            string recipeName = "Spaghetti Carbonara";
            int recipeCount = 1;

            // act
            HashSet<Recipe> recipes = bll.GetRecipes(null, recipeName);

            // assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }


        [TestMethod]
        public void GetRecipes_InvalidId_ReturnsNull()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int recipeId = 0;

            // act
            HashSet<Recipe> recipes = bll.GetRecipes(recipeId, null);

            // assert
            Assert.IsNull(recipes);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddRecipe_RecipeWithSameName_ThrowsException()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();

            RecipeWithIngredientsViewModel recipeWithIngredients = new RecipeWithIngredientsViewModel
            {
                Recipe = new Recipe { Name = "Spaghetti Carbonara" },
                Ingredients = new List<Ingredient>
                {
                    new Ingredient { Name = "Spaghetti" }
                }
            };

            HashSet<RecipeWithIngredientsViewModel> recipesWithIngredients = new HashSet<RecipeWithIngredientsViewModel>();
            recipesWithIngredients.Add(recipeWithIngredients);

            // Act
            bll.AddRecipes(recipesWithIngredients);
        }

        [TestMethod]
        public void AddRecipe_ExistingIngredient_AddNewRecipe()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int countCurrentRecipes = 12;

            RecipeWithIngredientsViewModel recipeWithIngredients = new RecipeWithIngredientsViewModel
            {
                Recipe = new Recipe { Name = "Spaghetti Alfredo style" },
                Ingredients = new List<Ingredient>
                {
                    new Ingredient { Name = "Spaghetti" }
                }
            };

            HashSet<RecipeWithIngredientsViewModel> recipesWithIngredients = new HashSet<RecipeWithIngredientsViewModel>();
            recipesWithIngredients.Add(recipeWithIngredients);

            // Act
            bll.AddRecipes(recipesWithIngredients);
            HashSet<Recipe> recipes = bll.GetRecipes(null, null);

            // Assert
            Assert.AreEqual(countCurrentRecipes + 1, recipes.Count);
        }

        /// Deletes an ingredient from the database. 
        /// If there is only one Recipe using that Ingredient, then the Recipe is also deleted, as well as all associated RecipeIngredients
        /// If there are multiple Recipes using that ingredient, a Forbidden response code should be provided with an appropriate message

        /*
         [TestMethod]
         public void DeleteIngredient_NoRecipesUsingIngredient_IngredientDeleted()
         {
             // Arrange
             BusinessLogicLayer bll = _initializeBusinessLogic();
             int ingredientId = 1;

             // Act
             var result = bll.DeleteAnIngredient(ingredientId, null);

             // Assert
             Assert.IsInstanceOfType(result, typeof(NoContentResult));
         }
         */
        [TestMethod]
        public void DeleteIngredient_IngredientNotFound_NotFoundResponse()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int ingredientId = 1;

            // Act
            var result = bll.DeleteAnIngredient(ingredientId, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


    }
}
