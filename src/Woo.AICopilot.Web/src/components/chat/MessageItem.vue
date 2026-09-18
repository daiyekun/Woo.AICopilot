<script setup lang="ts">
import { computed } from 'vue';
import { UserFilled, Service } from '@element-plus/icons-vue';
import { MessageRole } from '@/types/protocols.ts';
import BlockIntent from './BlockIntent.vue';
import BlockAnalysis from './BlockAnalysis.vue';
import BlockFinal from './BlockFinal.vue';
import type {ChatMessage, IntentChunk} from "@/types/models.ts";

const props = defineProps<{
  message: ChatMessage
}>();

const isUser = computed(() => props.message.role === MessageRole.User);

const intents = computed(() =>{
  const chunk = props.message.chunks.find(chunk => chunk.source === 'IntentRoutingExecutor') as IntentChunk;
  return chunk?.intents || [];
});

const dataChunks = computed(() =>
  props.message.chunks.filter(chunk => chunk.source === 'DataAnalysisExecutor') || []
);

const finalChunks = computed(() =>
  props.message.chunks.filter(chunk => chunk.source === 'FinalProcessExecutor' || chunk.source === 'User') || []
);
</script>

<template>
  <div class="message-row" :class="{ 'row-reverse': isUser }">
    <div class="avatar-container">
      <el-avatar
        :size="36"
        :icon="isUser ? UserFilled : Service"
        :class="isUser ? 'avatar-user' : 'avatar-ai'"
        :style="{ backgroundColor: isUser ? '#f0f0f0' : '#e1f3d8' }"
      />
    </div>

    <div class="content-container">
      <BlockIntent
        v-if="intents.length > 0"
        :intents="intents"/>

      <BlockAnalysis
        v-if="dataChunks.length > 0"
        :chunks="dataChunks"
        :is-streaming="message.isStreaming"/>

      <BlockFinal
        v-if="finalChunks.length > 0"
        :chunks="finalChunks"
        :is-user="isUser"
        :is-streaming="message.isStreaming"
      />
    </div>
  </div>
</template>

<style scoped>
.message-row { display: flex; margin-bottom: 20px; align-items: flex-start; }
.row-reverse { flex-direction: row-reverse; }
.avatar-container { margin: 0 10px; }
.content-container { max-width: 85%; display: flex; flex-direction: column; gap: 10px; }
.avatar-user { color: #666; }
.avatar-ai { background-color: #409eff; color: white; }
</style>
