<script setup lang="ts">
import { computed } from 'vue';
import type {StatsCardWidget} from "@/types/protocols.ts";

// 接收父组件传递的数据
const props = defineProps<{
  widget: StatsCardWidget
}>();

// 提取核心数据
const data = computed(() => props.widget.data);

// 格式化样式
const valueStyle = computed(() => ({
  color: '#303133',
  fontWeight: 'bold',
  fontSize: '24px'
}));
</script>

<template>
  <el-card shadow="hover" class="stats-card">
    <template #header>
      <div class="card-header">
        <span>{{ widget.title || data.label }}</span>
      </div>
    </template>

    <div class="card-content">
      <el-statistic
        :value="Number(data.value) || 0"
        :precision="2"
        :value-style="valueStyle"
      >
        <template #suffix>
          <span>{{ widget.data.unit }}</span>
        </template>
      </el-statistic>

    </div>
  </el-card>
</template>

<style scoped>
.stats-card {
  width: 240px; /* 固定宽度，看起来更整齐 */
  display: inline-block;
  margin-right: 12px;
  margin-bottom: 12px;
  border-radius: 8px;
}

.card-header {
  font-size: 14px;
  color: #606266;
  font-weight: 500;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
</style>
