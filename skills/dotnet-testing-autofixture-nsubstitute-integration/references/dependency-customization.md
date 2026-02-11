# 常見相依性的客製化處理

> 此文件從 [SKILL.md](../SKILL.md) 提取，包含 IMapper 等特殊相依性的客製化處理方式。

---

## IMapper 客製化（Mapster 範例）

某些相依性不適合使用 Mock，而應該使用真實實例：

```csharp
using AutoFixture;
using Mapster;
using MapsterMapper;

namespace MyProject.Tests.AutoFixtureConfigurations;

/// <summary>
/// Mapster 對應器客製化
/// </summary>
public class MapsterMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var typeAdapterConfig = new TypeAdapterConfig();
            typeAdapterConfig.Scan(typeof(ServiceMapRegister).Assembly);
            this._mapper = new Mapper(typeAdapterConfig);
            return this._mapper;
        }
    }
}
```

**為什麼 IMapper 不用 Mock？**

1. **工具型相依性**：Mapper 不是業務邏輯，是物件對應工具
2. **驗證對應邏輯**：測試需要驗證對應是否正確，Mock 會失去這個能力
3. **設定複雜度**：為每個對應方法設定 Returns 反而增加複雜度
4. **測試意圖**：我們要測試業務邏輯，不是 Mapper 的行為

## AutoMapper 客製化範例

```csharp
using AutoFixture;
using AutoMapper;

namespace MyProject.Tests.AutoFixtureConfigurations;

public class AutoMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });

            this._mapper = configuration.CreateMapper();
            return this._mapper;
        }
    }
}
```
