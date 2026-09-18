<script setup lang="ts">
import { Opportunity } from '@element-plus/icons-vue';
import type {IntentResult} from "@/types/protocols.ts";

defineProps<{
  intents: IntentResult[]
}>();

const getIntentColor = (confidence: number) => {
  if (confidence > 0.8) return 'success';
  if (confidence > 0.5) return 'warning';
  return 'danger';
};
</script>

<template>
  <div class="block-intent">
    <el-collapse :model-value="[]">
      <el-collapse-item name="intent">
        <template #title>
          <div class="intent-header">
            <el-icon><Opportunity /></el-icon>
            <span class="label">意图识别</span>
            <div class="intent-tags" v-if="intents.length > 0">
              <template v-for="(item) in intents">
                <el-tag
                  size="small"
                  :type="getIntentColor(item.confidence)"
                  effect="light"
                  class="ml-2 intent-tag"
                >
                  {{ item.intent }} {{ (item.confidence * 100).toFixed(0) }}%
                </el-tag>
              </template>
            </div>
          </div>
        </template>

        <div class="intent-body" v-if="intents.length > 0">
          <div v-for="(item, idx) in intents" :key="idx" class="intent-item">
            <div class="info-row">
              <span class="label">意图:</span>
              <span class="value"><strong>{{ item.intent }}</strong></span>
            </div>
            <div class="info-row" v-if="item.query">
              <span class="label">关键词:</span>
              <span class="value">{{ item.query }}</span>
            </div>
            <div class="info-row" v-if="item.reasoning">
              <span class="label">推理:</span>
              <span class="value">{{ item.reasoning }}</span>
            </div>
            <el-divider v-if="idx < intents.length - 1" class="intent-divider"/>
          </div>
        </div>
      </el-collapse-item>
    </el-collapse>
  </div>
</template>

<style scoped>
.block-intent { background: #fff; border-radius: 8px; border: 1px solid #ebeef5; overflow: hidden; }
.intent-header { display: flex; align-items: center; padding-left: 10px; color: #606266; width: 100%; }
.intent-tags { display: flex; flex-wrap: wrap; gap: 6px; flex: 1; margin-left: 10px; }
.label { font-size: 13px; font-weight: 500; margin-right: 8px; white-space: nowrap; }
.intent-body { padding: 10px 15px; background-color: #f9fafe; font-size: 13px; color: #555; }
.intent-divider { margin: 8px 0; border-top: 1px dashed #dcdfe6; }
.info-row { margin-bottom: 4px; display: flex; }
.info-row .label { color: #888; width: 60px; flex-shrink: 0; }

@keyframes rotating { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

/* 覆盖 Element Plus */
:deep(.el-collapse-item__header) { height: auto; min-height: 40px; border-bottom: 1px solid #ebeef5; }
:deep(.el-collapse-item__content) { padding-bottom: 0; }
:deep(.el-collapse) { border: none; }
</style>
