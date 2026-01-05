using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AutoTestSystem.DynamicProperty
{
    /// <summary>
    /// 動態屬性管理器，使用組合模式避免多重繼承問題
    /// </summary>
    public class DynamicPropertyManager
    {
        private readonly object _instance;
        private readonly Dictionary<string, Func<bool>> _propertyVisibilityRules;
        private DynamicTypeDescriptionProvider _provider;

        public DynamicPropertyManager(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _propertyVisibilityRules = new Dictionary<string, Func<bool>>();
        }

        /// <summary>
        /// 添加屬性可見性規則
        /// </summary>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="visibilityCondition">可見性條件</param>
        public void AddVisibilityRule(string propertyName, Func<bool> visibilityCondition)
        {
            _propertyVisibilityRules[propertyName] = visibilityCondition;
        }

        /// <summary>
        /// 批量添加屬性可見性規則
        /// </summary>
        /// <param name="rules">規則字典</param>
        public void AddVisibilityRules(Dictionary<string, Func<bool>> rules)
        {
            foreach (var rule in rules)
            {
                _propertyVisibilityRules[rule.Key] = rule.Value;
            }
        }

        /// <summary>
        /// 初始化動態屬性提供者
        /// </summary>
        public void Initialize()
        {
            if (_provider == null)
            {
                _provider = new DynamicTypeDescriptionProvider(_instance.GetType(), _instance, _propertyVisibilityRules);
                TypeDescriptor.AddProvider(_provider, _instance);
            }
        }

        /// <summary>
        /// 刷新屬性描述符
        /// </summary>
        public void RefreshProperties()
        {
            TypeDescriptor.Refresh(_instance);
        }

        /// <summary>
        /// 檢查屬性是否應該顯示
        /// </summary>
        public bool ShouldShowProperty(string propertyName)
        {
            return !_propertyVisibilityRules.ContainsKey(propertyName) ||
                   _propertyVisibilityRules[propertyName]?.Invoke() != false;
        }

        /// <summary>
        /// 移除動態屬性提供者
        /// </summary>
        public void Dispose()
        {
            if (_provider != null)
            {
                TypeDescriptor.RemoveProvider(_provider, _instance);
                _provider = null;
            }
        }
    }

    /// <summary>
    /// 動態類型描述提供者
    /// </summary>
    internal class DynamicTypeDescriptionProvider : TypeDescriptionProvider
    {
        private readonly TypeDescriptionProvider _defaultProvider;
        private readonly object _instance;
        private readonly Dictionary<string, Func<bool>> _visibilityRules;

        public DynamicTypeDescriptionProvider(Type objectType, object instance, Dictionary<string, Func<bool>> visibilityRules)
            : base(TypeDescriptor.GetProvider(objectType))
        {
            _defaultProvider = TypeDescriptor.GetProvider(objectType);
            _instance = instance;
            _visibilityRules = visibilityRules;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new DynamicTypeDescriptor(
                _defaultProvider.GetTypeDescriptor(objectType, instance),
                _instance,
                _visibilityRules);
        }
    }

    /// <summary>
    /// 動態類型描述符
    /// </summary>
    internal class DynamicTypeDescriptor : CustomTypeDescriptor
    {
        private readonly object _instance;
        private readonly Dictionary<string, Func<bool>> _visibilityRules;

        public DynamicTypeDescriptor(ICustomTypeDescriptor parent, object instance, Dictionary<string, Func<bool>> visibilityRules)
            : base(parent)
        {
            _instance = instance;
            _visibilityRules = visibilityRules;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = base.GetProperties(attributes).Cast<PropertyDescriptor>().ToList();

            if (_instance != null && _visibilityRules != null)
            {
                // 根據可見性規則過濾屬性
                properties.RemoveAll(p =>
                    _visibilityRules.ContainsKey(p.Name) &&
                    _visibilityRules[p.Name]?.Invoke() == false);
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }

    /// <summary>
    /// 動態屬性介面，可選擇性實作
    /// </summary>
    public interface IDynamicProperty
    {
        DynamicPropertyManager PropertyManager { get; }
        void ConfigurePropertyVisibility();
    }

    /// <summary>
    /// 擴展方法，讓任何物件都能使用動態屬性功能
    /// </summary>
    public static class DynamicPropertyExtensions
    {
        private static readonly Dictionary<object, DynamicPropertyManager> _managers =
            new Dictionary<object, DynamicPropertyManager>();

        /// <summary>
        /// 為物件取得或建立動態屬性管理器
        /// </summary>
        public static DynamicPropertyManager GetDynamicPropertyManager(this object instance)
        {
            if (!_managers.ContainsKey(instance))
            {
                _managers[instance] = new DynamicPropertyManager(instance);
            }
            return _managers[instance];
        }

        /// <summary>
        /// 快速設定屬性可見性規則
        /// </summary>
        public static void SetPropertyVisibility(this object instance, string propertyName, Func<bool> condition)
        {
            var manager = instance.GetDynamicPropertyManager();
            manager.AddVisibilityRule(propertyName, condition);
            manager.Initialize();
        }

        /// <summary>
        /// 快速刷新屬性
        /// </summary>
        public static void RefreshDynamicProperties(this object instance)
        {
            if (_managers.ContainsKey(instance))
            {
                _managers[instance].RefreshProperties();
            }
        }

        /// <summary>
        /// 清理動態屬性管理器
        /// </summary>
        public static void DisposeDynamicProperties(this object instance)
        {
            if (_managers.ContainsKey(instance))
            {
                _managers[instance].Dispose();
                _managers.Remove(instance);
            }
        }
    }

    /// <summary>
    /// 屬性可見性規則建構器，提供流暢的API
    /// </summary>
    public class PropertyVisibilityBuilder
    {
        private readonly Dictionary<string, Func<bool>> _rules = new Dictionary<string, Func<bool>>();

        public PropertyVisibilityBuilder Hide(string propertyName)
        {
            _rules[propertyName] = () => false;
            return this;
        }

        public PropertyVisibilityBuilder Show(string propertyName)
        {
            _rules[propertyName] = () => true;
            return this;
        }

        public PropertyVisibilityBuilder When(string propertyName, Func<bool> condition)
        {
            _rules[propertyName] = condition;
            return this;
        }

        public Dictionary<string, Func<bool>> Build()
        {
            return new Dictionary<string, Func<bool>>(_rules);
        }
    }
}