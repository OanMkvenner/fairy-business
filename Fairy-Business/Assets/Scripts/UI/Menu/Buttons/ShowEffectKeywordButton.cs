using System;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class ShowEffectKeywordButton : BaseButton
    {
        public static event Action<bool> OnEffectKeywordSelected;
        
        [SerializeField] private bool showEffectKeyword;
        
        protected override void OnClick()
        {
            OnEffectKeywordSelected?.Invoke(showEffectKeyword);
        }
    }
}