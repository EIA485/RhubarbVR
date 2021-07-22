﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using g3;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Render.Material.Fields;
using RhubarbEngine.Render.Shader;

namespace RhubarbEngine.Components.Assets
{
    [Category(new string[] { "Assets" })]
    public class RMaterial : AssetProvider<RMaterial>,IAsset
    {
        public AssetRef<RShader> Shader;

        public event Action<RMaterial> BindableResourcesReload;

        public List<ResourceLayoutElementDescription> resorses;

        public SyncAbstractObjList<MaterialField> Fields;

        public void ReloadBindableResources()
        {
            Logger.Log("Reload");
            BindableResourcesReload?.Invoke(this);
        }

        public override void onLoaded()
        {
            Logger.Log("Loaded Material");
            load(this);
        }

        public void getBindableResources(List<BindableResource> BindableResources,bool shadow=false)
        {
            foreach(var field in Shader.Asset.Fields)
            {
                var mitfield = getField<MaterialField>(field.fieldName, field.shaderType);
                if(mitfield == null)
                {
                    logger.Log(field.fieldName+"  :  " +field.shaderType.ToString() + "  :  " + field.valueType.ToString());
                    createField(field.fieldName, field.shaderType, field.valueType);
                    mitfield = getField<MaterialField>(field.fieldName, field.shaderType);
                }
                if (shadow)
                {
                    if ((int)mitfield.shaderType.value > 2)
                    {
                        if (mitfield.resource == null)
                        {
                            
                            throw new Exception($"resource is null shadow {shadow}");
                        }
                        else
                        {
                            BindableResources.Add(mitfield.resource);
                        }
                    }
                }
                else
                {
                    if ((int)mitfield.shaderType.value <= 2)
                    {
                        if (mitfield.resource == null)
                        {
                            throw new Exception($"resource is null nonshadow {shadow}");
                        }
                        else
                        {
                            BindableResources.Add(mitfield.resource);
                        }
                    }
                }

            }

        }

        public void setValueAtField<T>(string fieldName, ShaderType shaderType, T value)
        {
            foreach (MaterialField item in Fields)
            {
                if (item.fieldName.value == fieldName && item.shaderType.value == shaderType)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(typeof(T)))
                    {
                        item.setValue(((IWorldObject)value).ReferenceID);
                    }
                    else
                    {
                        item.setValue(value);
                    }
                    return;
                }
            }
        }
        public T getField<T>(string fieldName, ShaderType shaderType) where T: MaterialField
        {
            foreach (MaterialField item in Fields)
            {
                if (item.fieldName.value == fieldName && item.shaderType.value == shaderType)
                {
                    return (T)item;
                }
            }
            return null;
        }

        public void createField(string fieldName,ShaderType shader,ShaderValueType type)
        {
            Type vatype = typeof(MaterialField);
            switch (type)
            {
                case ShaderValueType.Val_bool:
                    vatype = typeof(BoolField);
                    break;
                case ShaderValueType.Val_int:
                    vatype = typeof(IntField);
                    break;
                case ShaderValueType.Val_uint:
                    vatype = typeof(UintField);
                    break;
                case ShaderValueType.Val_float:
                    vatype = typeof(FloatField);
                    break;
                case ShaderValueType.Val_double:
                    vatype = typeof(DoubleField);
                    break;
                case ShaderValueType.Val_bvec2:
                    vatype = typeof(Bvec2Field);
                    break;
                case ShaderValueType.Val_bvec3:
                    vatype = typeof(Bvec3Field);
                    break;
                case ShaderValueType.Val_bvec4:
                    vatype = typeof(Bvec4Field);
                    break;
                case ShaderValueType.Val_ivec2:
                    vatype = typeof(Ivec2Field);
                    break;
                case ShaderValueType.Val_ivec3:
                    vatype = typeof(Ivec3Field);
                    break;
                case ShaderValueType.Val_ivec4:
                    vatype = typeof(Ivec4Field);
                    break;
                case ShaderValueType.Val_uvec2:
                    vatype = typeof(Uvec2Field);
                    break;
                case ShaderValueType.Val_uvec3:
                    vatype = typeof(Uvec3Field);
                    break;
                case ShaderValueType.Val_uvec4:
                    vatype = typeof(Uvec4Field);
                    break;
                case ShaderValueType.Val_vec2:
                    vatype = typeof(Vec2Field);
                    break;
                case ShaderValueType.Val_vec3:
                    vatype = typeof(Vec3Field);
                    break;
                case ShaderValueType.Val_vec4:
                    vatype = typeof(Vec4Field);
                    break;
                case ShaderValueType.Val_dvec2:
                    vatype = typeof(Dvec2Field);
                    break;
                case ShaderValueType.Val_dvec3:
                    vatype = typeof(Dvec3Field);
                    break;
                case ShaderValueType.Val_dvec4:
                    vatype = typeof(Dvec4Field);
                    break;
                case ShaderValueType.Val_mat2x2:
                    break;
                case ShaderValueType.Val_mat3x2:
                    break;
                case ShaderValueType.Val_mat4x2:
                    break;
                case ShaderValueType.Val_mat2x3:
                    break;
                case ShaderValueType.Val_mat3x3:
                    break;
                case ShaderValueType.Val_mat4x3:
                    break;
                case ShaderValueType.Val_mat2x4:
                    break;
                case ShaderValueType.Val_mat3x4:
                    break;
                case ShaderValueType.Val_mat4x4:
                    break;
                case ShaderValueType.Val_color:
                    vatype = typeof(ColorField);
                    break;
                case ShaderValueType.Val_texture1D:
                    break;
                case ShaderValueType.Val_texture2D:
                    vatype = typeof(Texture2DField);
                    break;
                case ShaderValueType.Val_texture3D:
                    break;
                default:
                    break;
            }
            MaterialField newField = Fields.Add(vatype, true);
            newField.fieldName.value = fieldName;
            newField.shaderType.value = shader;
            newField.valueType.value = type;

        }

        public void LoadChange(RShader shader)
        {
            logger.Log("Starting Shader Uniform list");
            foreach (ShaderUniform item in shader.Fields)
            {
                bool val = false;
                foreach (MaterialField fildvalue in Fields)
                {
                    if (fildvalue.fieldName.value == item.fieldName)
                    {
                        if (fildvalue.shaderType.value != item.shaderType)
                        {
                            fildvalue.shaderType.value = item.shaderType;
                        }
                        if (fildvalue.valueType.value != item.valueType)
                        {
                            fildvalue.valueType.value = item.valueType;
                        }
                        val = true;
                    }
                }
                if (!val)
                {
                    logger.Log(item.fieldName + "  :  " + item.shaderType.ToString() + "  :  " + item.valueType.ToString());
                    createField(item.fieldName, item.shaderType, item.valueType);
                }
            }
            load(this);
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            Shader = new AssetRef<RShader>(this, newRefIds);
            Fields = new SyncAbstractObjList<MaterialField>(this, newRefIds);
            Shader.loadChange += LoadChange;
        }
        public RMaterial(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public RMaterial()
        {
        }
    }
}
