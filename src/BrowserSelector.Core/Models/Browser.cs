// <copyright file="Browser.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    /// ブラウザの種類を表す列挙型.
    /// </summary>
    public enum BrowserType
    {
        /// <summary>
        /// カスタムブラウザ
        /// </summary>
        Custom,

        /// <summary>
        /// Chrome
        /// </summary>
        Chrome,

        /// <summary>
        /// Firefox
        /// </summary>
        Firefox,

        /// <summary>
        /// Edge
        /// </summary>
        Edge,

        /// <summary>
        /// Safari
        /// </summary>
        Safari,

        /// <summary>
        /// Opera
        /// </summary>
        Opera,

        /// <summary>
        /// Internet Explorer
        /// </summary>
        InternetExplorer,

        /// <summary>
        /// Brave
        /// </summary>
        Brave,

        /// <summary>
        /// Vivaldi
        /// </summary>
        Vivaldi
    }

    /// <summary>
    /// ブラウザ情報を表すモデル.
    /// </summary>
    public partial class Browser : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _executablePath = string.Empty;

        [ObservableProperty]
        private string _iconPath = string.Empty;

        [ObservableProperty]
        private int _iconIndex = 0;

        [ObservableProperty]
        private string _arguments = string.Empty;

        [ObservableProperty]
        private bool _isDefault;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private int _displayOrder;

        [ObservableProperty]
        private DateTime _lastUsed = DateTime.MinValue;

        [ObservableProperty]
        private int _useCount;

        /// <summary>
        /// Gets or sets ブラウザの一意識別子.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets ブラウザの種類.
        /// </summary>
        public BrowserType Type { get; set; } = BrowserType.Custom;

        /// <summary>
        /// Gets a value indicating whether ブラウザが有効かどうかを判定.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(ExecutablePath);

        /// <summary>
        /// Gets ブラウザの表示名を取得.
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Unknown Browser" : Name;

        /// <summary>
        /// 使用回数を増加.
        /// </summary>
        public void IncrementUseCount()
        {
            UseCount++;
            LastUsed = DateTime.Now;
        }

        /// <summary>
        /// ブラウザの複製を作成.
        /// </summary>
        /// <returns></returns>
        public Browser Clone()
        {
            return new Browser
            {
                Id = Guid.NewGuid(), // 新しいIDを生成
                Name = Name,
                ExecutablePath = ExecutablePath,
                IconPath = IconPath,
                IconIndex = IconIndex,
                Arguments = Arguments,
                IsDefault = false, // 複製時はデフォルトをfalseにする
                IsEnabled = IsEnabled,
                DisplayOrder = DisplayOrder,
                Type = Type
            };
        }
    }
}
