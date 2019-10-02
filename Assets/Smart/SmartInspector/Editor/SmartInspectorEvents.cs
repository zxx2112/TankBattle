using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class SmartInspector
{
    GUIContent Label(string label)
    {
        return new GUIContent(label);
    }

    void ProcessEvent(Event e)
    {
        if (e.type != EventType.MouseDown) { return; }

        if (e.button != 1) { return; }

        GenericMenu menu = new GenericMenu();
        menu.AddItem(Label("Draw Header"), SISettings.DrawHeader, MenuHeaderItem);
        menu.AddItem(Label("Draw Titles"), SISettings.titles, MenuTitleItem);
        menu.AddItem(Label("Toggles View"), SISettings.toggles, MenuTogglesItem);
        menu.ShowAsContext();
    }

    // Header
    void MenuHeaderItem()
    {
        SISettings.DrawHeader = !SISettings.DrawHeader;
    }

    // Titles
    void MenuTitleItem()
    {
        SISettings.titles = !SISettings.titles;
    }
    
    // Toggles
    void MenuTogglesItem()
    {
        SISettings.toggles = !SISettings.toggles;
    }
}
