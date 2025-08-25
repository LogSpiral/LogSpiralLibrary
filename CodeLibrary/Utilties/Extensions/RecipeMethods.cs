namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

/// <summary>
/// 为什么还要给这个加个类？？
/// </summary>
public static class RecipeMethods
{
    public static void SetResult(this Recipe recipe, ModItem modItem, int stack = 1) => recipe.ReplaceResult(modItem.Type, stack);

    public static void SetResult(this Recipe recipe, int type, int stack = 1) => recipe.ReplaceResult(type, stack);

    public static void AddRecipe(this Recipe recipe) => recipe.Register();

    /// <summary>
    /// 坏处是没有ID提示
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="ingredients"></param>
    /// <returns></returns>
    public static Recipe QuickAddIngredient(this Recipe recipe, params int[] ingredients)
    {
        foreach (var item in ingredients)
        {
            recipe.AddIngredient(item);
        }
        return recipe;
    }

    /// <summary>
    /// 坏处是没有ID提示
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="ingredients"></param>
    /// <returns></returns>
    public static Recipe QuickAddIngredient(this Recipe recipe, params (int, int)[] ingredients)
    {
        foreach (var item in ingredients)
        {
            recipe.AddIngredient(item.Item1, item.Item2);
        }
        return recipe;
    }

    /// <summary>
    /// 坏处是没有ID提示
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="ingredients"></param>
    /// <returns></returns>
    public static Recipe QuickAddIngredient(this Recipe recipe, params ModItem[] ingredients)
    {
        foreach (var item in ingredients)
        {
            recipe.AddIngredient(item);
        }
        return recipe;
    }

    /// <summary>
    /// 坏处是没有ID提示
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="ingredients"></param>
    /// <returns></returns>
    public static Recipe QuickAddIngredient(this Recipe recipe, params (ModItem, int)[] ingredients)
    {
        foreach (var item in ingredients)
        {
            recipe.AddIngredient(item.Item1, item.Item2);
        }
        return recipe;
    }
}