<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick, computed } from 'vue';
import * as echarts from 'echarts';
import type {ChartWidget} from "@/types/protocols.ts";

const props = defineProps<{
  widget: ChartWidget
}>();

// DOM 引用：用于挂载 Canvas
const chartRef = ref<HTMLElement | null>(null);
// ECharts 实例引用
let chartInstance: echarts.ECharts | null = null;
// ResizeObserver 实例
let resizeObserver: ResizeObserver | null = null;

// ================== 核心逻辑：数据转换 (Adapter) ==================

/**
 * 将后端 DTO 转换为 ECharts Option
 * 这是连接业务数据与可视化库的桥梁
 */
const getChartOption = () => {
  const chartType = props.widget.data.category;
  const { x, y, seriesName } = props.widget.data.encoding;
  const title = props.widget.title;

  // 1. 通用基础配置
  const baseOption: any = {
    title: {
      text: title,
      left: 'center',
      textStyle: { fontSize: 14, color: '#333' }
    },
    tooltip: {
      trigger: chartType === 'Pie' ? 'item' : 'axis', // 饼图触发方式不同
      confine: true // 将 Tooltip 限制在图表容器内
    },
    legend: {
      bottom: 0, // 图例放在底部
      type: 'scroll' // 图例过多时允许滚动
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%', // 留出空间给图例
      containLabel: true
    }
  };

  // 2. 根据图表类型构建配置
  // 后端返回的 chartType 首字母大写 (Bar, Line, Pie)，ECharts 需要小写 (bar, line, pie)
  const typeLower = chartType.toLowerCase();

  if (chartType === 'Pie') {
    // ---- 饼图逻辑 ----
    return {
      ...baseOption
    }
  } else {
    // ---- 柱状图 / 折线图逻辑 ----
    return {
      ...baseOption
    };
  }
};

// ================== 生命周期管理 ==================

/**
 * 初始化图表
 */
const initChart = () => {
  if (!chartRef.value) return;

  // 初始化实例，并应用 light 主题
  chartInstance = echarts.init(chartRef.value, null, { renderer: 'canvas' });

  // 设置数据
  try {
    const option = getChartOption();
    chartInstance.setOption(option);
  } catch (e) {
    console.error('ECharts Option Error:', e);
  }
};

/**
 * 处理窗口大小变化
 * 使用 ResizeObserver 比 window.onresize 更精确，能监听到 div 本身的变化
 */
const setupResizeObserver = () => {
  if (!chartRef.value) return;

  resizeObserver = new ResizeObserver(() => {
    chartInstance?.resize();
  });
  resizeObserver.observe(chartRef.value);
};

onMounted(async () => {
  // 等待 DOM 渲染完成
  await nextTick();
  initChart();
  setupResizeObserver();
});

onUnmounted(() => {
  // 销毁资源
  resizeObserver?.disconnect();
  chartInstance?.dispose();
});

// 监听数据变化（虽然目前是一次性渲染，但保留此逻辑支持实时更新）
watch(() => props.widget, () => {
  if (chartInstance) {
    chartInstance.setOption(getChartOption(), true); // true 表示不合并，完全重置
  }
}, { deep: true });

</script>

<template>
  <div class="chart-container">
    <div ref="chartRef" class="echarts-dom"></div>
  </div>
</template>

<style scoped>
.chart-container {
  width: 100%;
  max-width: 600px; /* 限制最大宽度，防止在大屏上太宽 */
  background: #fff;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
  padding: 16px;
  margin-top: 8px;
}

.echarts-dom {
  width: 100%;
  height: 350px; /* 固定高度，确保 Canvas 有渲染空间 */
}
</style>
