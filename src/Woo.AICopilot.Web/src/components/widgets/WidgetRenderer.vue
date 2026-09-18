<script setup lang="ts">
import { defineAsyncComponent } from 'vue';
import type {Widget} from "@/types/protocols.ts";

const props = defineProps<{
  data: Widget
}>();

// 异步加载组件 (Code Splitting)
// 这样做的好处是，如果用户从未收到 Chart 类型的消息，ChartWidget 的代码就不会被加载
const ChartWidget = defineAsyncComponent(() => import('./ChartWidget.vue'));
const StatsWidget = defineAsyncComponent(() => import('./StatsWidget.vue'));
const DataTableWidget = defineAsyncComponent(() => import('./DataTableWidget.vue'));

// 简单的类型映射字典 (可选，也可以用 v-if/else-if)
// 这里为了简单直观，我们在模板中直接判断
</script>

<template>
  <div class="widget-renderer">
    <ChartWidget
      v-if="data.type === 'Chart'"
      :widget="(data as any)"
    />

    <StatsWidget
      v-else-if="data.type === 'StatsCard'"
      :widget="(data as any)"
    />

    <DataTableWidget
      v-else-if="data.type === 'DataTable'"
      :widget="(data as any)"
    />

    <div v-else class="unknown-widget">
      <el-alert
        :title="`暂不支持的组件类型: ${data.type}`"
        type="warning"
        show-icon
        :closable="false"
      />
      <pre class="debug-data">{{ data }}</pre>
    </div>
  </div>
</template>

<style scoped>
.widget-renderer {
  margin-top: 10px;
  width: 100%;
}

.unknown-widget {
  margin-top: 8px;
}

.debug-data {
  background: #f4f4f5;
  padding: 8px;
  border-radius: 4px;
  font-size: 11px;
  color: #909399;
  overflow-x: auto;
}
</style>
