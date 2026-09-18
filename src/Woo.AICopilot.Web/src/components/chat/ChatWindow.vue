<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useChatStore } from '@/stores/chatStore.ts';
import SessionList from './SessionList.vue';
import MessageItem from './MessageItem.vue';
import { Promotion } from '@element-plus/icons-vue';

const store = useChatStore();
const inputValue = ref('');
const scrollContainer = ref<HTMLElement | null>(null);

// 发送消息处理
const handleSend = async () => {
  const content = inputValue.value.trim();
  if (!content || store.isStreaming) return;

  // 清空输入框
  inputValue.value = '';

  // 调用 Store 发送
  await store.sendMessage(content);
};

// 滚动到底部逻辑
const scrollToBottom = async () => {
  await nextTick(); // 等待 Vue DOM 更新完成
  if (scrollContainer.value) {
    scrollContainer.value.scrollTop = scrollContainer.value.scrollHeight;
  }
};

// 监听消息列表变化，自动滚动
// deep: true 确保能监听到数组内部元素的变化（如流式传输时的内容追加）
watch(
  () => store.currentMessages,
  () => {
    scrollToBottom();
  },
  { deep: true }
);

// 切换会话时也要滚动到底部
watch(
  () => store.currentSessionId,
  () => {
    scrollToBottom();
  }
);
</script>

<template>
  <div class="chat-layout">
    <div class="sidebar-wrapper">
      <SessionList />
    </div>

    <div class="main-wrapper">
      <header class="chat-header">
        <h2>智能助手</h2>
      </header>

      <main class="chat-viewport" ref="scrollContainer">
        <div class="messages-list">
          <div v-if="store.currentMessages.length === 0" class="welcome-banner">
            <h3>👋 欢迎使用 .NET AI Copilot</h3>
            <p>我可以帮您分析数据、查询库存，请试着问我：</p>
            <div class="suggestion-chips">
              <el-tag @click="inputValue='分析各仓库库存占比';handleSend()" class="chip">分析各仓库库存占比</el-tag>
              <el-tag @click="inputValue='查询最近一周的销售趋势'" class="chip">查询销售趋势</el-tag>
            </div>
          </div>

          <MessageItem
            v-for="msg in store.currentMessages"
            :key="msg.timestamp"
            :message="msg"
          />
        </div>
      </main>

      <footer class="chat-input-area">
        <div class="input-container">
          <el-input
            v-model="inputValue"
            type="textarea"
            :autosize="{ minRows: 1, maxRows: 4 }"
            placeholder="输入您的问题 (Enter 发送, Shift+Enter 换行)..."
            @keydown.enter.prevent="(e:KeyboardEvent) => { if(!e.shiftKey) handleSend() }"
            :disabled="store.isStreaming"
          />
          <el-button
            type="primary"
            class="send-btn"
            :disabled="!inputValue.trim() || store.isStreaming"
            @click="handleSend"
          >
            <el-icon><Promotion /></el-icon>
          </el-button>
        </div>
        <div class="footer-tip">
          AI 生成的内容可能不准确，请核实重要信息。
        </div>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.chat-layout {
  display: flex;
  height: 100%;
  width: 100%;
  overflow: hidden;
}

.sidebar-wrapper {
  width: 260px;
  flex-shrink: 0;
}

.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-color-primary);
  position: relative;
}

.chat-header {
  height: 60px;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  padding: 0 24px;
  font-weight: 600;
}

.chat-viewport {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  scroll-behavior: smooth; /* 平滑滚动 */
}

.messages-list {
  max-width: 800px; /* 限制内容最大宽度，提升阅读体验 */
  margin: 0 auto;
}

.chat-input-area {
  padding: 24px;
  border-top: 1px solid var(--border-color);
  background-color: #fff;
}

.input-container {
  max-width: 800px;
  margin: 0 auto;
  display: flex;
  gap: 12px;
  align-items: flex-end; /* 底部对齐 */
}

.send-btn {
  height: 40px;
  width: 40px;
  border-radius: 8px;
}

.footer-tip {
  text-align: center;
  color: var(--text-secondary);
  font-size: 12px;
  margin-top: 8px;
}

.welcome-banner {
  text-align: center;
  margin-top: 100px;
  color: var(--text-secondary);
}

.suggestion-chips {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  gap: 10px;
}

.chip {
  cursor: pointer;
}
</style>
