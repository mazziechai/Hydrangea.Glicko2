# Hydrangea.Glicko2
Another C# implementation of Glicko-2 with a focus on extensibility. I created this library for my ranked matchmaking Discord bot.

## How to use

First, you need to create a Glicko2 object. You can optionally set certain properties you wish to change in an object initializer, like this:
```cs
var glicko2 = new Glicko2() { VolatilityConstraint = 0.75 };
```
Then, you can use this new Glicko2 object to create ratings, add them to the rating period, and then update them.

```cs
// InitializeRating() uses the default variables in the Glicko2 object, which you can change during initialization.
var rating1, rating2 = glicko2.InitializeRating();

// Now that you have your ratings, you can report a result. rating1 is the winner, rating2 is the loser.
glicko2.RecordResult(rating1, rating2);

// Finally, you can update those ratings. 
glicko2.UpdateRatings();
```

Since this implementation is intended to be flexible, you can make any adjustments or do anything different as you'd like.
