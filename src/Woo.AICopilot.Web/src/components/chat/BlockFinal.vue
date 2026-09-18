<script setup lang="ts">
import {renderMarkdown} from '@/utils/markdown';
import FunctionCallItem from './FunctionCallItem.vue';
import {type ChatChunk, ChunkType} from "@/types/protocols.ts";
import type {FunctionCallChunk} from "@/types/models.ts";

const props = defineProps<{
  chunks: ChatChunk[]
  isUser: boolean;
  isStreaming: boolean;
}>();

const getFunctionCall = (chunk : ChatChunk): FunctionCallChunk =>
  chunk as FunctionCallChunk;

</script>

<template>
  <div class="block-final message-bubble" :class="isUser ? 'bubble-user' : 'bubble-ai'">
    <template v-for="chunk in chunks">
      <div
        v-if="chunk.type === ChunkType.Text"
        class="markdown-body inline-block-container"
        v-html="renderMarkdown(chunk.content)"
      ></div>

      <div
        v-else-if="chunk.type === ChunkType.FunctionCall"
        class="my-1 inline-block"
      >
        <FunctionCallItem
          :call="getFunctionCall(chunk).functionCall"
          :mini="true"
        />
      </div>

      <span v-if="isStreaming" class="cursor-blink">|</span>
    </template>
  </div>
</template>

<style scoped>
.message-bubble {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 14px;
  line-height: 1.6;
  position: relative;
  word-break: break-word;
}

/* 用户气泡 */
.bubble-user {
  background-color: #95ec69;
  color: #000;
}

/* AI 气泡 */
.bubble-ai {
  background-color: #fff;
  border: 1px solid #e4e7ed;
  color: #333;
}

.cursor-blink {
  display: inline-block;
  width: 2px;
  height: 14px;
  background: #333;
  animation: blink 1s infinite;
  vertical-align: middle;
  margin-left: 2px;
}

.inline-block { display: inline-block; }
.inline-block-container { display: inline-block; width: 100%; }
.my-1 { margin: 4px 0; }

/* 修正 markdown 里的 p 标签 margin，使其在 steps 拼接时更自然 */
:deep(.markdown-body p:last-child) { margin-bottom: 0; }
:deep(.markdown-body p:first-child) { margin-top: 0; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
</style>
