using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MaterialRename;
[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    private static string _oldString;
    private static string _newString;
 
    private StringBuilder _failures = new StringBuilder();
    
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        if (commandData.Application.ActiveUIDocument.Document is null)
        {
            throw new ArgumentException("activedoc");
        }
        else
        {
            App.RevitDocument = commandData.Application.ActiveUIDocument.Document;
        }

        //TODO create a formed to ket the oldstring newstring values.
        var form = new Forms.MainForm();
        if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _oldString = form.textBoxFind.Text;
            _newString = form.textBoxReplace.Text;
        }

        //get all the materials from the active document
        FilteredElementCollector fec = new FilteredElementCollector(App.RevitDocument);
        fec.OfClass(typeof(Material));

        IEnumerable<Material> materials = fec.Cast<Material>();

        Transaction trans = null;

        try
        {
            trans = new Transaction(App.RevitDocument, "Rename materials and assets");
            trans.Start();
            
            //loop through all the materials and rename them
            foreach (Material material in materials)
            {
                material.Name.Replace(_oldString, _newString);

                //get the material asset elements
                AppearanceAssetElement elementAppearance = (AppearanceAssetElement)App.RevitDocument.GetElement(material.AppearanceAssetId);
                PropertySetElement elementPhysical = (PropertySetElement)App.RevitDocument.GetElement(material.StructuralAssetId);
                PropertySetElement elementThermal = (PropertySetElement)App.RevitDocument.GetElement(material.ThermalAssetId);

                //get the material asset names
                if (elementAppearance != null)
                {
                    //rename the material asset
                    Debug.WriteLine($"Apperance Asset : {elementAppearance.Name}");                  
                    elementAppearance.Name = elementAppearance.Name.Replace(_oldString, _newString);
                    ChangeRenderingAssetTextureName( elementAppearance,_oldString, _newString);
                }
                if (elementPhysical != null)
                {
                    //rename the material asset
                    elementPhysical.Name = elementPhysical.Name.Replace(_oldString, _newString);
                }
                if (elementThermal != null)
                {
                    //rename the material asset
                    elementThermal.Name = elementThermal.Name.Replace(_oldString, _newString);
                }

            }
                        
            trans.Commit();
            TaskDialog.Show("MaterialRename", $"Materials and assets processed. Check the following \n {_failures.ToString()}");
    }
        catch (Exception ex)
        {
            
            trans.RollBack();
            TaskDialog.Show("MaterialRename", ex.Message);
        }

        return Result.Succeeded;
    }
    
    private void ChangeRenderingAssetTextureName(AppearanceAssetElement appearanceAsset, string oldString, string newString)
    {
        //get rendering asset of appearanceAsset
        //Asset asset = appearanceAsset.GetRenderingAsset();

        using (AppearanceAssetEditScope editScope = new AppearanceAssetEditScope(App.RevitDocument))
        {
            // returns an editable copy of the appearance asset
            Asset editableAsset = editScope.Start(appearanceAsset.Id);

            RenameAsset(editableAsset, "opaque_albedo", UnifiedBitmap.UnifiedbitmapBitmap, oldString, newString);
            RenameAsset(editableAsset, "opaque_f0", UnifiedBitmap.UnifiedbitmapBitmap, oldString, newString);
            RenameAsset(editableAsset, "metal_f0", UnifiedBitmap.UnifiedbitmapBitmap, oldString, newString);
            RenameAsset(editableAsset, "surface_albedo", UnifiedBitmap.UnifiedbitmapBitmap, oldString, newString);
            RenameAsset(editableAsset, "surface_roughness", UnifiedBitmap.UnifiedbitmapBitmap, oldString, newString);
            RenameAsset(editableAsset, "surface_normal", BumpMap.BumpmapBitmap, oldString, newString);        

            editScope.Commit(true);
        }
    }

    private void RenameAsset(Asset asset, string propertyName, string connectedPropertyName, string oldString, string newString)
    {
        AssetProperty textureMapProperty = asset.FindByName(propertyName);
        if (textureMapProperty != null)
        {
            Asset connectedAsset = textureMapProperty.GetSingleConnectedAsset();
            if (connectedAsset != null)
            {
                AssetPropertyString texturemapBitmapProperty = connectedAsset.FindByName(connectedPropertyName) as AssetPropertyString;
                if(texturemapBitmapProperty != null)
                {
                    try
                    {
                        var newValue = Regex.Replace(texturemapBitmapProperty.Value, oldString, newString, RegexOptions.IgnoreCase); // texturemapBitmapProperty.Value.Replace(oldString, newString);
                        texturemapBitmapProperty.Value = newValue;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        _failures.Append($"{asset.Name} : {texturemapBitmapProperty.Value} \n");
                    }
                }                
            }
        }
    }

    private void ReadAsset(Asset asset)
    {
        // Get the asset name, type and library name.
        AssetType type = asset.AssetType;
        string name = asset.Name;
        string libraryName = asset.LibraryName;

        Debug.WriteLine($"Asset: {name} : {type} : {libraryName}");

        // travel the asset properties in the asset.
        for (int idx = 0; idx < asset.Size; idx++)
        {
            AssetProperty prop = asset.Get(idx);
            ReadAssetProperty(prop);
        }
    }

    private void ReadAssetProperty(AssetProperty prop)
    {
        switch (prop.Type)
        {
            //// Retrieve the value from simple type property is easy.
            //// for example, retrieve bool property value.
            //case AssetPropertyType.Boolean:
            //    AssetPropertyBoolean boolProp = prop as AssetPropertyBoolean;
            //    bool propValue = boolProp.Value;
            //    Debug.WriteLine($"Property: {prop.Name} : {propValue}");
            //    break;

            //// When you retrieve the value from the data array property,
            //// you may need to get which value the property stands for.
            //// for example, the APT_Double44 may be a transform data.
            //case AssetPropertyType.Double44:
            //    AssetPropertyDoubleArray4d transformProp = prop as AssetPropertyDoubleArray4d;
            //    IList<double> tranformValue = transformProp.GetValueAsDoubles();
            //    break;

            //// The APT_List contains a list of sub asset properties with same type.
            //case AssetPropertyType.List:
            //    AssetPropertyList propList = prop as AssetPropertyList;
            //    IList<AssetProperty> subProps = propList.GetValue();
            //    if (subProps.Count == 0)
            //        break;
            //    switch (subProps[0].Type)
            //    {
            //        case AssetPropertyType.Integer:
            //            foreach (AssetProperty subProp in subProps)
            //            {
            //                AssetPropertyInteger intProp = subProp as AssetPropertyInteger;
            //                int intValue = intProp.Value;
            //            }
            //            break;
            //    }
            //    break;

            case AssetPropertyType.String:
                AssetPropertyString stringProp = prop as AssetPropertyString;
                string stringValue = stringProp.Value;
                Debug.WriteLine($"Property: {prop.Name} : {stringValue}");
                break;
                
            case AssetPropertyType.Asset:
                Asset propAsset = prop as Asset;
                ReadAsset(propAsset);
                Debug.WriteLine($"Nested asset: {prop.Name}");
                break;
            default:
                break;
        }

        // Get the connected properties.
        // please notice that the information of many texture stores here.
        if (prop.NumberOfConnectedProperties == 0)
            return;
        
        foreach (AssetProperty connectedProp in prop.GetAllConnectedProperties())
        {
            // Note: Usually, the connected property is an Asset.
            ReadAssetProperty(connectedProp);
        }

    }
    
    private void ProcessAsset(Asset asset, string oldString, string newString)
    {
        // Get the asset name, type and library name.
        AssetType type = asset.AssetType;
        string name = asset.Name;
        string libraryName = asset.LibraryName;

        Debug.WriteLine($"Asset: {name} : {type} : {libraryName}");

        // travel the asset properties in the asset.
        for (int idx = 0; idx < asset.Size; idx++)
        {
            AssetProperty prop = asset.Get(idx);
            //ReadAssetProperty(prop);
            ChangeAssetPropertyValue(prop, oldString, newString);
        }
    }

    private void ChangeAssetPropertyValue(AssetProperty prop, string oldString, string newString)
    {
        switch (prop.Type)
        {
             case AssetPropertyType.String:
                AssetPropertyString stringProp = prop as AssetPropertyString;
                string stringValue = stringProp.Value;
                Debug.WriteLine($"Property: {prop.Name} : {stringValue}");
                 
                
                
                break;

            case AssetPropertyType.Asset:
                Asset propAsset = prop as Asset;
                ProcessAsset(propAsset, oldString, newString);
                Debug.WriteLine($"Nested asset: {prop.Name}");
                break;
                
            default:
                break;
        }

        // Get the connected properties.
        // please notice that the information of many texture stores here.
        if (prop.NumberOfConnectedProperties == 0)
            return;

        foreach (AssetProperty connectedProp in prop.GetAllConnectedProperties())
        {
                // Note: Usually, the connected property is an Asset.
                ChangeAssetPropertyValue(connectedProp, oldString, newString);
        }

    }
}
