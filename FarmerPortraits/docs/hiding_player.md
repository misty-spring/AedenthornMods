# Hiding the player

As of 1.3, you can use the key `$no_player` to hide portraits on world dialogues.

## How to use

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
        "HasMod": "mistyspring.aedenthornFarmerPortraits",
        "mistyspring.aedenthornFarmerPortraits/ShowMisc": true
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