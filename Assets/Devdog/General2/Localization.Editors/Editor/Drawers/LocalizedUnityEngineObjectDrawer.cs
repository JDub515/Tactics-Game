﻿using System;
using System.Reflection;
using Devdog.General2.Editors.ReflectionDrawers;
using UnityEditor;

namespace Devdog.General2.Localization.Editors
{
    [CustomDrawer(typeof(LocalizedObject))]
    public class LocalizedUnityEngineObjectDrawer : LocalizedObjectDrawerBase<UnityEngine.Object, LocalizedObject>
    {
        public LocalizedUnityEngineObjectDrawer(FieldInfo fieldInfo, object value, object parentValue, int arrayIndex)
            : base(fieldInfo, value, parentValue, arrayIndex)
        {
        }

        public override DrawerBase FindDrawerRelative(string fieldName)
        {
            return null;
        }
    }
}
