using Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;

namespace Woo.AICopilot.MigrationWorkApp.SeedData;

public static class DataAnalysisData
{
    public static IEnumerable<BusinessDatabase> GetDatabases()
    {
        // 模拟一个 ERP 数据库连接
        // 注意：在实际开发环境中，这里应该指向一个真实存在的测试库
        // 本示例假设本地有一个名为 'erp_demo' 的 PostgreSQL 数据库
        var erpDb = new BusinessDatabase(
            "ERP_Core",
            "负责管理产品主数据、客户关系与销售业务。包含：SKU基础信息、价格表、客户档案、销售订单（订单号/金额/状态）以及账面库存总量。当用户询问“卖了多少”、“谁买的”、“订单金额”、“还有多少库存（总量）”时，请选择此库。",
            "Host=localhost;Port=5432;Database=erp_demo;Username=postgres;Password=123456",
            DbProviderType.PostgreSql
        );  
        
        return [erpDb];
    }
}