<script setup lang="ts">
import { computed, ref } from 'vue';
import type { DataTableWidget } from "@/types/protocols.ts";
import { Filter, Setting } from '@element-plus/icons-vue';

const props = defineProps<{
  widget: DataTableWidget
}>();

// ================== 状态管理 ==================

// 控制列的显示/隐藏：存储当前选中的列 Key
const visibleColumnKeys = ref<string[]>([]);

// 初始化：默认显示所有列
if (props.widget.data.columns) {
  visibleColumnKeys.value = props.widget.data.columns.map(c => c.key);
}

// 搜索关键词 (可选扩展)
const searchQuery = ref('');

// ================== 计算属性 ==================

/**
 * 最终用于渲染的列配置
 * 根据 visibleColumnKeys 过滤
 */
const displayColumns = computed(() => {
  return props.widget.data.columns.filter(col => visibleColumnKeys.value.includes(col.key));
});

/**
 * 表格数据
 * Element Plus Table 自带排序和筛选，这里直接透传 rows
 * 如果需要前端全局搜索，可以在这里对 props.widget.data.rows 进行 filter
 */
const tableData = computed(() => {
  let data = props.widget.data.rows;
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    data = data.filter(row =>
      Object.values(row).some(val => String(val).toLowerCase().includes(query))
    );
  }
  return data;
});

// ================== 辅助函数 ==================

/**
 * 获取某一列的所有唯一值，用于生成筛选菜单
 */
const getColumnFilters = (columnKey: string) => {
  const values = new Set(props.widget.data.rows.map(row => row[columnKey]));
  return Array.from(values).map(val => ({ text: String(val), value: val }));
};

/**
 * 筛选处理函数
 */
const filterHandler = (value: string, row: any, column: any) => {
  const property = column['property'];
  return row[property] === value;
};

/**
 * 格式化单元格内容
 */
const formatCell = (row: any, column: any, cellValue: any, index: number) => {
  const colDef = props.widget.data.columns.find(c => c.key === column.property);
  if (!colDef || cellValue == null) return cellValue;

  if (colDef.dataType === 'date') {
    // 简单日期格式化，实际项目中建议用 dayjs
    try {
      return new Date(cellValue).toLocaleDateString();
    } catch { return cellValue; }
  }

  if (colDef.dataType === 'number') {
    // 数字千分位
    return Number(cellValue).toLocaleString();
  }

  return cellValue;
};

</script>

<template>
  <div class="data-table-widget">
    <div class="widget-header">
      <div class="title-area">
        <h3 class="widget-title">{{ widget.title }}</h3>
        <span class="widget-desc" v-if="widget.description">{{ widget.description }}</span>
      </div>

      <div class="actions">
        <el-input
          v-model="searchQuery"
          placeholder="Search..."
          size="small"
          style="width: 150px; margin-right: 8px;"
          clearable
        />

        <el-popover placement="bottom-end" :width="200" trigger="click">
          <template #reference>
            <el-button :icon="Setting" circle size="small" />
          </template>
          <div class="column-settings">
            <div class="settings-title">Visible Columns</div>
            <el-checkbox-group v-model="visibleColumnKeys" direction="vertical">
              <div v-for="col in widget.data.columns" :key="col.key" class="setting-item">
                <el-checkbox :value="col.key" :label="col.label" />
              </div>
            </el-checkbox-group>
          </div>
        </el-popover>
      </div>
    </div>

    <div class="table-content">
      <el-table
        :data="tableData"
        style="width: 100%"
        height="300"
        stripe
        border
        size="small"
      >
        <el-table-column
          v-for="col in displayColumns"
          :key="col.key"
          :prop="col.key"
          :label="col.label"
          :min-width="120"
          sortable
          resizable
          :filters="getColumnFilters(col.key)"
          :filter-method="filterHandler"
          :formatter="formatCell"
        />

        <template #empty>
          <el-empty description="No Data" :image-size="60" />
        </template>
      </el-table>
    </div>
  </div>
</template>

<style scoped>
.data-table-widget {
  width: 100%;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
  padding: 12px;
  margin-top: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  display: flex;
  flex-direction: column;
}

.widget-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.title-area {
  display: flex;
  flex-direction: column;
}

.widget-title {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.widget-desc {
  font-size: 12px;
  color: #909399;
}

.column-settings {
  max-height: 250px;
  overflow-y: auto;
}

.settings-title {
  font-size: 12px;
  font-weight: bold;
  color: #606266;
  margin-bottom: 8px;
  padding-bottom: 4px;
  border-bottom: 1px solid #EBEEF5;
}

.setting-item {
  margin-bottom: 4px;
}
</style>
