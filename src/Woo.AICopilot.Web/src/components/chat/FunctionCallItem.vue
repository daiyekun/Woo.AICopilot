<script setup lang="ts">
import { computed } from 'vue';
import {
  CircleCheck,
  CircleClose,
  Loading,
  Operation // 通用图标
} from '@element-plus/icons-vue';
import type {FunctionCall} from "@/types/models.ts";

const props = defineProps<{
  call: FunctionCall;
  mini?: boolean; // 迷你模式（用于最终回复块）
}>();

// --- 状态计算 ---
const isRunning = computed(() => props.call.status === 'calling');
const isSuccess = computed(() => props.call.status === 'completed');

// --- 样式配置 ---
const statusColor = computed(() => {
  if (isRunning.value) return '#E6A23C'; // 橙色：执行中
  if (isSuccess.value) return '#67C23A'; // 绿色：成功
  return '#909399';
});

const statusIcon = computed(() => {
  if (isRunning.value) return Loading;
  if (isSuccess.value) return CircleCheck;
  return CircleClose;
});

const statusText = computed(() => {
  if (isRunning.value) return '正在执行...';
  if (isSuccess.value) return '调用成功';
  return '调用失败';
});

// 格式化参数显示 (尝试格式化 JSON，失败则显示原文本)
const formattedArgs = computed(() => {
  try {
    const json = JSON.parse(props.call.args);
    return JSON.stringify(json, null, 2);
  } catch (e) {
    return props.call.args;
  }
});
</script>

<template>
  <div
    v-if="mini"
    class="fc-mini"
    :class="{ 'is-running': isRunning, 'is-success': isSuccess }"
  >
    <div class="fc-mini-header">
      <el-icon class="icon-indicator" :class="{ 'is-loading': isRunning }">
        <component :is="statusIcon" />
      </el-icon>
      <span class="fc-name font-mono">{{ call.name }}()</span>
    </div>
  </div>

  <div
    v-else
    class="fc-item"
    :class="{
      'border-running': isRunning,
      'border-success': isSuccess
    }"
  >
    <el-collapse>
      <el-collapse-item>
        <template #title>
          <div class="fc-header">
            <div
              class="status-icon-box"
              :style="{
                borderColor: statusColor,
                backgroundColor: isRunning ? '#fff' : statusColor,
                color: isRunning ? statusColor : '#fff'
              }"
            >
              <el-icon :class="{ 'is-loading': isRunning }">
                <component :is="statusIcon" />
              </el-icon>
            </div>

            <div class="fc-info">
              <div class="fc-title-row">
                <span class="fc-label">Function Call:</span>
                <span class="fc-name">{{ call.name }}</span>
              </div>
              <div class="fc-status-text" :style="{ color: statusColor }">
                {{ statusText }}
              </div>
            </div>

            <div v-if="isRunning" class="running-indicator">
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
            </div>
          </div>
        </template>

        <div class="fc-details">
          <div class="detail-section">
            <div class="section-label">Parameters (Input):</div>
            <pre class="code-block json-content">{{ formattedArgs }}</pre>
          </div>

          <div class="detail-section mt-2">
            <div class="section-label">Execution Result:</div>

            <div v-if="isRunning" class="result-loading">
              <el-icon class="is-loading"><Loading /></el-icon>
              <span>Waiting for output...</span>
            </div>

            <pre v-else class="code-block result-content">{{ call.result || 'No return value' }}</pre>
          </div>
        </div>
      </el-collapse-item>
    </el-collapse>
  </div>
</template>

<style scoped>
/* === 通用 === */
.font-mono { font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; }

/* === 完整模式样式 === */
.fc-item {
  margin-bottom: 8px;
  background: #fff;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
  transition: all 0.3s ease; /* 平滑过渡 */
  overflow: hidden;
}

/* 状态边框 & 呼吸动画 */
.border-running {
  border-color: #f3d19e;
  box-shadow: 0 0 8px rgba(230, 162, 60, 0.2);
  animation: pulse-border 2s infinite;
}
.border-success {
  border-color: #e1f3d8;
  border-left: 4px solid #67C23A; /* 左侧加粗强调 */
}
.border-failed {
  border-color: #fde2e2;
  border-left: 4px solid #F56C6C;
}

/* Header 布局 */
.fc-header {
  display: flex;
  align-items: center;
  width: 100%;
  padding-left: 10px;
  height: 100%;
}

.status-icon-box {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 1px solid;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  font-size: 14px;
  transition: all 0.3s;
}

.fc-info {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}
.fc-title-row {
  display: flex;
  align-items: center;
  font-size: 13px;
  color: #303133;
}
.fc-label {
  color: #909399;
  margin-right: 6px;
  font-size: 12px;
}
.fc-name {
  font-weight: 600;
  font-family: monospace;
  color: #409eff;
}
.fc-status-text {
  font-size: 11px;
}

/* 详情区 */
.fc-details {
  padding: 12px;
  background: #fafafa;
  border-top: 1px solid #ebeef5;
}
.detail-section { margin-bottom: 8px; }
.section-label {
  font-size: 11px;
  text-transform: uppercase;
  color: #909399;
  margin-bottom: 4px;
  font-weight: 600;
}
.code-block {
  margin: 0;
  font-family: monospace;
  font-size: 12px;
  white-space: pre-wrap;
  word-break: break-all;
  padding: 8px;
  border-radius: 4px;
  border: 1px solid #e4e7ed;
}
.json-content { background-color: #fff; color: #606266; }
.result-content { background-color: #f0f9eb; color: #529b2e; border-color: #e1f3d8; }

/* Result Loading 占位符 */
.result-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #909399;
  font-size: 12px;
  padding: 8px;
  background: #fff;
  border: 1px dashed #dcdfe6;
  border-radius: 4px;
}

/* === 迷你模式样式 === */
.fc-mini {
  display: inline-flex;
  align-items: center;
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 12px;
  border: 1px solid #e9e9eb;
  background: #f4f4f5;
  color: #909399;
  margin-right: 8px;
}
.fc-mini-header { display: flex; align-items: center; gap: 6px; }

.fc-mini.is-running {
  background: #fdf6ec;
  border-color: #faecd8;
  color: #e6a23c;
}
.fc-mini.is-success {
  background: #f0f9eb;
  border-color: #e1f3d8;
  color: #67c23a;
}
.fc-mini.is-failed {
  background: #fef0f0;
  border-color: #fde2e2;
  color: #f56c6c;
}

/* === 动画 === */
@keyframes pulse-border {
  0% { box-shadow: 0 0 0 0 rgba(230, 162, 60, 0.4); }
  70% { box-shadow: 0 0 0 6px rgba(230, 162, 60, 0); }
  100% { box-shadow: 0 0 0 0 rgba(230, 162, 60, 0); }
}

/* Element Plus Override */
:deep(.el-collapse) { border: none; }
:deep(.el-collapse-item__header) {
  height: auto;
  min-height: 48px;
  padding: 6px 0;
  border-bottom: none;
  background: transparent;
}
:deep(.el-collapse-item__wrap) { border-bottom: none; }
:deep(.el-collapse-item__content) { padding-bottom: 0; }

/* 运行中的三个点动画 */
.running-indicator {
  margin-left: auto;
  margin-right: 10px;
  display: flex;
  gap: 3px;
}
.dot {
  width: 4px;
  height: 4px;
  background-color: #e6a23c;
  border-radius: 50%;
  animation: bounce 1.4s infinite ease-in-out both;
}
.dot:nth-child(1) { animation-delay: -0.32s; }
.dot:nth-child(2) { animation-delay: -0.16s; }
@keyframes bounce { 0%, 80%, 100% { transform: scale(0); } 40% { transform: scale(1); } }
</style>
