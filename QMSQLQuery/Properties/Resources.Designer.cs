﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuickMon.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("QuickMon.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap doc_add {
            get {
                object obj = ResourceManager.GetObject("doc_add", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap doc_edit {
            get {
                object obj = ResourceManager.GetObject("doc_edit", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap doc_refresh {
            get {
                object obj = ResourceManager.GetObject("doc_refresh", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap doc_remove {
            get {
                object obj = ResourceManager.GetObject("doc_remove", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Download {
            get {
                object obj = ResourceManager.GetObject("Download", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap satelitedish {
            get {
                object obj = ResourceManager.GetObject("satelitedish", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;config&gt;
        ///  &lt;queries&gt;
        ///    &lt;!--
        ///    &lt;query name=&quot;&quot; sqlServer=&quot;.&quot; database=&quot;master&quot; integratedSec=&quot;True&quot; userName=&quot;&quot;  password=&quot;&quot;
        ///      usePersistentConnection=&quot;False&quot; applicationName=&quot;QuickMon&quot;
        ///       cmndTimeOut=&quot;60&quot;&gt;
        ///      &lt;summaryQuery useSP=&quot;False&quot; returnValueIsNumber=&quot;True&quot; returnValueInverted=&quot;False&quot;
        ///                  warningValue=&quot;1&quot; errorValue=&quot;2&quot; successValue=&quot;[any]&quot;
        ///                  useRowCountAsValue=&quot;False&quot; useExecuteTimeAsValue=&quot;False&quot;&gt;select * from SomeTable&lt;/summaryQuery&gt;
        ///      &lt;detai [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlQueryEmptyConfig {
            get {
                return ResourceManager.GetString("SqlQueryEmptyConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html lang=&quot;en-za&quot; xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///&lt;title&gt;QuickMon SQL Query Collector tips&lt;/title&gt;
        ///&lt;style type=&quot;text/css&quot;&gt;
        ///    body 
        ///    {
        ///        font-family: Verdana;
        ///        font-size: 12;
        ///    }
        ///    h1, h2
        ///    {
        ///        color: Maroon;
        ///    }
        ///    a
        ///    {
        ///        text-decoration: none;
        ///    }
        ///    a:hover
        ///    {
        ///        text-decoration: underline:
        ///    }
        ///&lt;/style&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///&lt;h1&gt;QuickMon SQL Query Collector&lt;/h1&gt;
        ///&lt;p&gt;The &lt;a href=&quot;http://quickmon.codeplex.com&quot; target=&quot;_bla [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SQLQueryTips {
            get {
                return ResourceManager.GetString("SQLQueryTips", resourceCulture);
            }
        }
    }
}
