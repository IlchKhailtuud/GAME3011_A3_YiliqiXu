using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DifficultySO : ScriptableObject
{
   private Difficulty gameGameDifficulty;

   public Difficulty GameDifficulty
   {
      get => gameGameDifficulty;
      set => gameGameDifficulty = value;
   }
}
