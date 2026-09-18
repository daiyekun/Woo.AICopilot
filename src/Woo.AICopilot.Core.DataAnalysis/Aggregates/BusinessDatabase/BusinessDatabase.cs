using Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;
using Woo.AICopilot.SharedKernel.Domain;

/// <summary>
/// 业务数据库聚合根
/// 代表一个可被AI Agent访问的外部数据源
/// </summary>
public class BusinessDatabase : IAggregateRoot
{
    protected BusinessDatabase() { }

    public BusinessDatabase(
        string name, 
        string description, 
        string connectionString, 
        DbProviderType provider)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        ConnectionString = connectionString;
        Provider = provider;
        IsEnabled = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    /// <summary>
    /// 数据库标识名称
    /// 用于在多库路由时作为唯一Key
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// 数据库业务描述 (如: "包含所有销售订单、客户资料及发货记录")
    /// 该字段将被注入到System Prompt中，辅助LLM进行意图路由判断
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; private set; } = null!;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbProviderType Provider { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新连接信息
    /// </summary>
    public void UpdateConnection(string connectionString, DbProviderType provider)
    {
        ConnectionString = connectionString;
        Provider = provider;
    }

    /// <summary>
    /// 更新描述信息
    /// </summary>
    public void UpdateInfo(string name, string description)
    {
        Name = name;
        Description = description;
    }
}