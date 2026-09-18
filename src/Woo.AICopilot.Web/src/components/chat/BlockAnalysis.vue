<script setup lang="ts">
import {DataLine, Monitor} from '@element-plus/icons-vue';
import {renderMarkdown} from '@/utils/markdown';
import FunctionCallItem from './FunctionCallItem.vue';
import {type ChatChunk, ChunkType} from "@/types/protocols.ts";
import type {FunctionCallChunk, WidgetChunk} from "@/types/models.ts";
import WidgetRenderer from '../widgets/WidgetRenderer.vue';

defineProps<{
  chunks: ChatChunk[]
  isStreaming: boolean
}>();

const getFunctionCall = (chunk: ChatChunk): FunctionCallChunk =>
  chunk as FunctionCallChunk;

const getWidget = (chunk: ChatChunk): WidgetChunk =>
  chunk as WidgetChunk;

</script>

<template>
  <div class="block-analysis">
    <div class="analysis-card">
      <div class="analysis-header">
        <el-icon class="header-icon"><DataLine /></el-icon>
        <span>数据分析与决策</span>
        <span v-if="isStreaming" class="typing-dot">...</span>
      </div>
      <div class="analysis-content">
        <template v-for="chunk in chunks">
          <div
            v-if="chunk.type === ChunkType.Text"
            class="markdown-body text-analysis mb-3"
            v-html="renderMarkdown(chunk.content)"
          ></div>

          <div v-else-if="chunk.type === ChunkType.FunctionCall" class="mb-3">
            <FunctionCallItem
            :call="getFunctionCall(chunk).functionCall"
            />
          </div>

          <div v-else-if="chunk.type === ChunkType.Widget" class="mb-3 widget-wrapper">
            <WidgetRenderer :data="getWidget(chunk).widget" />
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
.block-analysis { width: 100%; }
.analysis-card { background: #fafafa; border: 1px solid #dcdfe6; border-radius: 8px; overflow: hidden; }

/* 头部样式 */
.analysis-header {
  background: #f2f6fc;
  padding: 8px 12px;
  display: flex;
  align-items: center;
  color: #409eff;
  font-size: 13px;
  font-weight: 600;
  border-bottom: 1px solid #ebeef5;
}
.header-icon { margin-right: 6px; }

/* 内容区域 */
.analysis-content { padding: 12px; }
.text-analysis { font-size: 14px; color: #444; line-height: 1.6; }

/* 间距控制 */
.mb-3 { margin-bottom: 12px; }

/* Widget 容器样式 */
.widget-wrapper {
  margin-top: 5px;
  width: 100%;             /* 占满父容器 */
  max-width: 100%;         /* 限制最大宽度 */
  overflow-x: auto;        /* 关键：允许内部横向滚动 */
  -webkit-overflow-scrolling: touch; /* 移动端顺滑滚动 */
}

/* 动画 */
.typing-dot { animation: blink 1.5s infinite; margin-left: 4px; }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
</style>
