namespace EatThis.Api.Domain;

public enum RestaurantCategory
{
    TaiwaneseChinese,
    Japanese,
    Korean,
    HotPot,
    Barbecue,
    Italian,
    BreakfastBrunch,
    FastFood,
    Vegetarian,
    CafeDessert,
}

public static class RestaurantCategoryParser
{
    public static bool TryParse(string? code, out RestaurantCategory? category)
    {
        category = code switch
        {
            "taiwanese-chinese" => RestaurantCategory.TaiwaneseChinese,
            "japanese" => RestaurantCategory.Japanese,
            "korean" => RestaurantCategory.Korean,
            "hot-pot" => RestaurantCategory.HotPot,
            "barbecue" => RestaurantCategory.Barbecue,
            "italian" => RestaurantCategory.Italian,
            "breakfast-brunch" => RestaurantCategory.BreakfastBrunch,
            "fast-food" => RestaurantCategory.FastFood,
            "vegetarian" => RestaurantCategory.Vegetarian,
            "cafe-dessert" => RestaurantCategory.CafeDessert,
            _ => null,
        };
        return code is null || category.HasValue;
    }
}
