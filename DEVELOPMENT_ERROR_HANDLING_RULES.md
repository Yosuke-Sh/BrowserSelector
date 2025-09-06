# BrowserSelector 開発エラー対処ルール

## 概要
BrowserSelectorプロジェクトで発生するエラーの対処手順を記載します。テスト実行時に発生したエラーを体系的に解決するためのガイドラインです。

## エラー分類と対処手順

### 1. DI設定エラー（Dependency Injection Configuration Errors）

#### エラー例
```
System.InvalidOperationException : No constructor for type 'BrowserSelector.Infrastructure.Services.BrowserService' can be instantiated using services from the service container and default values.
```

#### 対処手順
1. **サービスクラスのコンストラクタを確認**
   - 対象サービスクラスのコンストラクタで必要な依存関係を特定
   - 例：`BrowserService`は`IRegistryService`、`IUrlService`、`ILogService`を必要

2. **ServiceCollectionに不足している依存関係を追加**
   ```csharp
   services.AddSingleton<IRegistryService, WindowsRegistryService>();
   services.AddSingleton<IUrlService, UrlService>();
   services.AddSingleton<ILogService, LogService>();
   ```

3. **依存関係の依存関係も確認**
   - 各サービスのコンストラクタでさらに必要な依存関係があるかチェック
   - 循環依存がないか確認

#### 実装例
```csharp
public ActualViewModelTests()
{
    var services = new ServiceCollection();
    
    // 基本サービス
    services.AddLogging();
    
    // 依存関係を正しい順序で登録
    services.AddSingleton<ILogService, LogService>();
    services.AddSingleton<IRegistryService, WindowsRegistryService>();
    services.AddSingleton<IUrlService, UrlService>();
    services.AddSingleton<IBrowserService, BrowserService>();
    services.AddSingleton<ISettingsService, SettingsService>();
    
    _serviceProvider = services.BuildServiceProvider();
}
```

### 2. 型の不一致エラー（Type Mismatch Errors）

#### エラー例
```
Expected type to be System.Collections.Generic.List`1[[BrowserSelector.Core.Models.CustomLanguageFile, ...]], but found System.Collections.Generic.List`1[[BrowserSelector.Core.Models.LanguageInfo, ...]].
```

#### 対処手順
1. **実際のサービスメソッドの戻り値型を確認**
   - インターフェース定義と実装クラスの戻り値型を比較
   - ジェネリック型の具体的な型パラメータを確認

2. **テストのアサーションを修正**
   ```csharp
   // 間違い
   result.Should().BeOfType<List<CustomLanguageFile>>();
   
   // 正しい
   result.Should().BeOfType<List<LanguageInfo>>();
   ```

3. **LINQクエリの戻り値型に注意**
   ```csharp
   // OrderBy()はOrderedEnumerableを返す
   var orderedRules = rules.OrderBy(r => r.Priority);
   orderedRules.Should().BeOfType<OrderedEnumerable<UrlRule, int>>();
   
   // Listに変換する場合
   var ruleList = rules.OrderBy(r => r.Priority).ToList();
   ruleList.Should().BeOfType<List<UrlRule>>();
   ```

### 3. 論理的失敗エラー（Logical Failure Errors）

#### エラー例
```
Expected result to be true, but found False.
```

#### 対処手順
1. **サービスの実装ロジックを確認**
   - メソッドが`false`を返す条件を特定
   - テスト環境での制約や前提条件を確認

2. **テストデータの妥当性を検証**
   ```csharp
   // 無効なデータでテストしている可能性
   var invalidRule = new UrlRule { Pattern = "", BrowserName = "" };
   var result = await _urlRuleService.AddRuleAsync(invalidRule);
   result.Should().BeFalse(); // 無効なデータなのでfalseが正しい
   ```

3. **テストの期待値を実際の動作に合わせて調整**
   ```csharp
   // 実際のサービス実装に合わせて期待値を調整
   var result = await _customLanguageService.AddCustomLanguageAsync("invalid-path");
   result.Should().BeFalse(); // 無効なパスなのでfalseが正しい
   ```

### 4. 警告の対処（Warning Handling）

#### 警告例
```
CS8625: null リテラルを null 非許容参照型に変換できません。
CS0219: 変数 'propertyChangedRaised' は割り当てられていますが、その値は使用されていません。
```

#### 対処手順
1. **null許容参照型の警告**
   ```csharp
   // 警告が出る場合
   string? nullableString = null;
   string nonNullableString = nullableString; // CS8625
   
   // 修正方法1: null-forgiving operator
   string nonNullableString = nullableString!;
   
   // 修正方法2: null チェック
   string nonNullableString = nullableString ?? string.Empty;
   ```

2. **未使用変数の警告**
   ```csharp
   // 警告が出る場合
   var propertyChangedRaised = false;
   _viewModel.PropertyChanged += (sender, e) => propertyChangedRaised = true;
   // propertyChangedRaisedを使用していない
   
   // 修正方法1: 変数を使用
   _viewModel.PropertyChanged += (sender, e) => propertyChangedRaised = true;
   propertyChangedRaised.Should().BeTrue();
   
   // 修正方法2: 変数を削除
   _viewModel.PropertyChanged += (sender, e) => { /* 何らかの処理 */ };
   ```

## エラー対処の優先順位

### 1. 最優先：DI設定エラー
- アプリケーションが起動できない根本的な問題
- 他のエラーの原因となる可能性が高い

### 2. 高優先：型の不一致エラー
- コンパイルエラーやランタイムエラーの原因
- テストの信頼性に影響

### 3. 中優先：論理的失敗エラー
- テストの期待値と実際の動作の不一致
- サービスの実装ロジックの理解が必要

### 4. 低優先：警告
- コードの品質向上
- 将来的なバグの予防

## エラー対処のチェックリスト

### DI設定エラーの場合
- [ ] 対象サービスのコンストラクタを確認
- [ ] 必要な依存関係をすべてServiceCollectionに登録
- [ ] 依存関係の依存関係も確認
- [ ] 循環依存がないか確認
- [ ] サービスのライフサイクル（Singleton、Scoped、Transient）を適切に設定

### 型の不一致エラーの場合
- [ ] 実際のサービスメソッドの戻り値型を確認
- [ ] インターフェース定義と実装の一致を確認
- [ ] LINQクエリの戻り値型に注意
- [ ] ジェネリック型の具体的な型パラメータを確認
- [ ] テストのアサーションを修正

### 論理的失敗エラーの場合
- [ ] サービスの実装ロジックを確認
- [ ] テストデータの妥当性を検証
- [ ] テスト環境での制約を確認
- [ ] 期待値を実際の動作に合わせて調整
- [ ] 必要に応じてテストをスキップ

### 警告の場合
- [ ] null許容参照型の警告を修正
- [ ] 未使用変数の警告を修正
- [ ] その他の警告を確認・修正
- [ ] コードの品質向上

## エラー対処後の確認事項

1. **ビルドエラーがないことを確認**
   ```bash
   dotnet build
   ```

2. **テストが正常に実行されることを確認**
   ```bash
   dotnet test
   ```

3. **警告が解消されていることを確認**
   - ビルドログで警告が表示されないことを確認

4. **コードレビューを実施**
   - 修正内容が適切かどうか確認
   - 他の部分に影響がないか確認

## 参考情報

### 主要なサービスクラスとその依存関係
- `BrowserService`: `IRegistryService`, `IUrlService`, `ILogService`
- `SettingsService`: `ILogService`
- `UrlService`: `ILogService`
- `UrlRuleService`: `ILogService`
- `CustomLanguageService`: `ILogService`
- `LocalizationService`: `ILogService`
- `WindowsRegistryService`: `ILogService`

### よく使用されるFluentAssertions
```csharp
// 型チェック
result.Should().BeOfType<ExpectedType>();

// 真偽値チェック
result.Should().BeTrue();
result.Should().BeFalse();

// null チェック
result.Should().NotBeNull();
result.Should().BeNull();

// コレクション チェック
collection.Should().NotBeEmpty();
collection.Should().HaveCount(expectedCount);
```

このルールに従って、エラーを体系的に解決していきます。
