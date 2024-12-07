# Hiding the player

As of 1.3, you can hide the player's portraits on world dialogues.

## How to use

### New method

You can just use an empty character at any point in the dialogue, and the portrait will be hidden.
It must be this specific character: ‎` ‎`

It's suggested to use it at the end. Like this:

`"It works like this.#$b#%They start explaining it to you.#$#%You feel your eyes glaze over... ‎"` 

The empty character is hidden at the end, so it won't affect how your text looks, and you don't need to use any tokens.

![Example video. A farmer player and Penny talk. She says "Hi. There should be no player here.‏ Neither here." The panels, accordingly, have no player portrait.](https://github.com/misty-spring/AedenthornMods/blob/master/FarmerPortraits/docs/example_new.gif)

### Legacy method
First, add this to your dynamic tokens:
```jsonc
"DynamicTokens": [
    {
      "Name": "n",
      "Value":""
    },
    {
      "Name":"n",
      "Value":"$no_player ",
      "When": {
        "HasMod": "mistyspring.aedenthornFarmerPortraits"
      }
    }
  ],
```

now, you can use the key to hide your player on a specific dialogue. For example:

```json
{
  "Action": "EditData",
  "Target": "Strings/StringsFromCSFiles",
  "Entries": {
    "GameLocation.cs.8429": "There's no mail.#{{n}}How strange...#But it could arrive!"
  }
}
```

If the player has the portrait enabled for world dialogue, the portrait will be hidden. Otherwise, the dialogue will show normally (without any command).

This also makes sure that the dialogue will show normally if they don't have Farmer Portraits installed.

![Example video. A character interacts with the mailbox, and a dialogue pops up.](https://github.com/misty-spring/AedenthornMods/blob/master/FarmerPortraits/docs/example.gif)
