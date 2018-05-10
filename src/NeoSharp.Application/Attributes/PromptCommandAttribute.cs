﻿using NeoSharp.Core.Extensions;
using NeoSharp.Core.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace NeoSharp.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PromptCommandAttribute : Attribute
    {
        #region Constants

        private static readonly Type _stringType = typeof(string);
        private static readonly Type _iListType = typeof(IList);
        private static readonly Type _fileInfoType = typeof(FileInfo);
        private static readonly Type _directoryInfoType = typeof(DirectoryInfo);
        private static readonly Type _objArrayType = typeof(object[]);

        private static readonly char[] _splitChars = { ';', ',', '|' };

        #endregion

        #region Variables

        private ParameterInfo[] _parameters;
        private MethodInfo _method;

        #endregion

        #region Properties

        /// <summary>
        /// Commands
        /// </summary>
        public readonly string[] Commands;
        /// <summary>
        /// Help
        /// </summary>
        public string Help { get; set; }
        /// <summary>
        /// Method
        /// </summary>
        internal MethodInfo Method
        {
            get { return _method; }
            set
            {
                if (value == null) return;

                _method = value;
                _parameters = value.GetParameters();
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commands">Commands</param>
        public PromptCommandAttribute(params string[] commands)
        {
            Commands = commands;
        }

        /// <summary>
        /// Convert string arguments to Method arguments
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Return parsed arguments</returns>
        public object[] ConvertToArguments(CommandToken[] args)
        {
            var max = _parameters.Length;
            var ret = new object[max];

            if (args.Length < max)
                throw (new ArgumentException("Missing parameters"));

            for (var x = 0; x < max; x++)
            {
                if (_parameters[x].GetCustomAttribute<PromptCommandParameterBodyAttribute>() != null)
                {
                    // From here to the end

                    ret[x] = ParseToArgument(new CommandToken(string.Join(" ", args.Skip(x)), false), _parameters[x].ParameterType);
                    return ret;
                }
                else
                {
                    // Regular parameter

                    ret[x] = ParseToArgument(args[x], _parameters[x].ParameterType);
                }
            }

            return ret;
        }

        object ParseAutoObject(CommandToken token)
        {
            if (!token.Quoted)
            {
                if (token.Value.StartsWith("0x"))
                {
                    return token.Value.HexToBytes();
                }
                else
                {
                    // Number?

                    if (BigInteger.TryParse(token.Value, out BigInteger bi))
                        return bi;

                    // Decimal?

                    if (BigDecimal.TryParse(token.Value, 20, out BigDecimal bd))
                        return bd;

                    // TODO: Parse address format here
                }
            }

            return token.Value;
        }

        object ParseAutoObject(string value)
        {
            List<object> ret = new List<object>();
            List<object> curArray = null;

            // Separate Array tokens

            List<CommandToken> tks = new List<CommandToken>();

            foreach (CommandToken token in value.SplitCommandLine().ToArray())
            {
                if (token.Quoted) tks.Add(token);
                else
                {
                    string val = token.Value;
                    if (val.StartsWith("["))
                    {
                        tks.Add(new CommandToken("["));
                        val = val.Substring(1);
                    }

                    CommandToken add = null;

                    if (val.EndsWith("]"))
                    {
                        add = new CommandToken("]");
                        val = val.Substring(0, val.Length - 1);
                    }

                    if (!string.IsNullOrEmpty(val))
                        tks.Add(new CommandToken(val, false));

                    if (add != null)
                        tks.Add(add);
                }
            }

            // Fetch parameters

            foreach (CommandToken token in tks)
            {
                string val = token.Value;

                if (token.Quoted)
                {
                    object oc = ParseAutoObject(token);

                    if (curArray != null) curArray.Add(oc);
                    else ret.Add(oc);
                }
                else
                {
                    switch (val)
                    {
                        case "[":
                            {
                                curArray = new List<object>();
                                break;
                            }
                        case "]":
                            {
                                if (curArray != null)
                                {
                                    ret.Add(curArray.ToArray());
                                    curArray = null;
                                }
                                break;
                            }
                        default:
                            {
                                object oc = ParseAutoObject(token);

                                if (curArray != null) curArray.Add(oc);
                                else ret.Add(oc);
                                break;
                            }
                    }
                }
            }

            if (curArray != null) throw new ArgumentException();

            return ret.Count == 1 ? ret[0] : ret.ToArray();
        }

        /// <summary>
        /// Parse argument
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="type">Type</param>
        /// <returns>Return parsed argument</returns>
        private object ParseToArgument(CommandToken token, Type type)
        {
            // Auto-detect
            if (_objArrayType == type)
            {
                return ParseAutoObject(token.Value);
            }

            // FileInfo
            if (_fileInfoType == type)
            {
                return new FileInfo(token.Value);
            }

            // DirectoryInfo
            if (_directoryInfoType == type)
            {
                return new DirectoryInfo(token.Value);
            }

            // Array
            if (type.IsArray)
            {
                var l = new List<object>();
                var gt = type.GetElementType();
                foreach (var ii in token.Value.Split(_splitChars))
                {
                    var ov = ParseToArgument(new CommandToken(ii, false), gt);
                    if (ov == null) continue;

                    l.Add(ov);
                }

                var a = (Array)Activator.CreateInstance(type, l.Count);
                Array.Copy(l.ToArray(), a, l.Count);
                return a;
            }

            // List
            if (_iListType.IsAssignableFrom(type))
            {
                var l = (IList)Activator.CreateInstance(type);

                // If dosen't have T return null
                if (type.GenericTypeArguments == null || type.GenericTypeArguments.Length == 0)
                    return null;

                var gt = type.GenericTypeArguments[0];
                foreach (var ii in token.Value.Split(_splitChars))
                {
                    var ov = ParseToArgument(new CommandToken(ii, false), gt);
                    if (ov == null) continue;

                    l.Add(ov);
                }
                return l;
            }

            // Is Convertible
            var conv = TypeDescriptor.GetConverter(type);
            if (conv.CanConvertFrom(_stringType))
            {
                return conv.ConvertFrom(token.Value);
            }

            throw (new ArgumentException());
        }
    }
}