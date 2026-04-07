using FGB.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FGB.Dominio.Extensoes
{
    public static class AgregacoesTools
    {
        public static EntidadeBase VincularColecoes(this EntidadeBase source)
        {
            if (source == null)
            {
                return source;
            }
            var propriedades = source.GetType().GetProperties();
            foreach (var atributo in propriedades)
            {
                var tipoAtributo = atributo.PropertyType;
                if (tipoAtributo.IsGenericList())
                {
                    var colecao = (IList)atributo.GetValue(source)!;

                    if (colecao == null || colecao.Count == 0)
                    {
                        continue;
                    }

                    Type type = null;
                    foreach (var item in colecao)
                    {
                        if (item != null)
                        {
                            type = item.GetType();
                            break;
                        }
                    }

                    if (type == null)
                    {
                        continue;
                    }

                    var sourceType = source.GetType().Name;
                    var property = type.GetProperty(sourceType);
                    if (property == null)
                    {
                        continue;
                    }
                    foreach (var item in colecao)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        property.SetValue(item, source);
                        if (item is EntidadeBase entidadeBase)
                        {
                            entidadeBase.VincularColecoes(); //Vincular Colecoes Recursivamente
                            entidadeBase.UltimaAlteracao = DateTime.Now;
                        }
                    }
                }
            }
            return source;
        }
    }

    public static class TypeTools
    {
        public static bool IsGenericList(this Type source)
        {
            var isGenericList = source.IsGenericType && (source.GetGenericTypeDefinition() == typeof(IList<>));
            return isGenericList;
        }
    }
}
