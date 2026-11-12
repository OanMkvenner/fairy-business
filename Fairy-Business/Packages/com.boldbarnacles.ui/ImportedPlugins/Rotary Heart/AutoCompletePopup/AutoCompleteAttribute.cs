using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RotaryHeart.Lib.AutoComplete
{
    public class AutoCompleteAttribute : PropertyAttribute
    {
        public string[] Entries;
        public Type baseType = null;
        
        public AutoCompleteAttribute(string[] entries)
        {
            Entries = entries;
        }
        public AutoCompleteAttribute(Type baseType)
        {
            this.baseType = baseType;
            Entries = null; //
        }
        public string[] GetEntries(){
            if (Entries == null || Entries.Count() == 0){
                //Entries = GetAvailableEntriesOfType(baseType).ToArray();
                Entries = GetAvailableEntriesCardSurvival(baseType).ToArray();
            }
            return Entries;
        }
        static public List<string> GetAvailableEntriesOfType(Type baseType){
            var dropdownList = new List<string>();
            var foundTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t => t != baseType /*&& t != typeof(SoundBaseSO) */ && baseType.IsAssignableFrom(t));
            //var foundTypes = Assembly.ReflectionOnlyLoad("Library/ScriptAssemblies/Assembly-CSharp.dll").GetTypes().Where(
            //    t => t != baseType /*&& t != typeof(SoundBaseSO) */ && baseType.IsAssignableFrom(t)
            //);
            //dropdownList.Add("None Selected");
            foreach (var item in foundTypes)
            {
                dropdownList.Add(item.Name);
            }
            dropdownList.Sort();
            return dropdownList;
        }
        static public List<string> GetAvailableEntriesCardSurvival(Type baseType){
            var foundTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes()).Where(t => t != baseType /*&& t != typeof(SoundBaseSO) */ && baseType.IsAssignableFrom(t)).ToList();
            var foundTypesAsStrings = foundTypes.Select(x => x.Name).ToList();
            var allQualityDefaults = Resources.LoadAll("AllScriptableObjects/Definitions/QualityDefaults").ToList();
            foundTypesAsStrings.AddRange(allQualityDefaults.Select(x => x.name).ToList());
            foundTypesAsStrings.Sort((x, y) => {
                var firstType = x.ToString();
                var secondType = y.ToString();
                int priorityCheck = secondType.StartsWith("TQ_").CompareTo(firstType.StartsWith("TQ_"));
                if (priorityCheck != 0) return priorityCheck;
                priorityCheck = secondType.StartsWith("Q_").CompareTo(firstType.StartsWith("Q_"));
                if (priorityCheck != 0) return priorityCheck;
                return x.ToString().CompareTo(y.ToString());
            });
            var dropdownList = new List<string>();
            dropdownList.Add("None Selected");
            foundTypesAsStrings.Remove("None Selected");
            foreach (var item in foundTypesAsStrings)
            {
                dropdownList.Add(item);
            }
            return dropdownList;
        }
    }
    
    public class AutoCompleteTextFieldAttribute : AutoCompleteAttribute
    {
        public AutoCompleteTextFieldAttribute(string[] entries) : base(entries) { }
        public AutoCompleteTextFieldAttribute(Type baseType) : base(baseType) { }
    }
}