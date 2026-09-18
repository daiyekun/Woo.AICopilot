<script setup lang="ts">
import { onMounted } from 'vue';
import { useChatStore } from './stores/chatStore';
import ChatWindow from './components/chat/ChatWindow.vue';

const chatStore = useChatStore();

// 应用启动时，自动获取历史会话
onMounted(async () => {
  await chatStore.init();
  // 如果没有会话，自动创建一个新的，避免空白
  if (chatStore.sessions.length === 0) {
    await chatStore.createNewSession();
  }
});
</script>

<template>
  <div class="app-container">
    <ChatWindow />
  </div>
</template>

<style scoped>
.app-container {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Inter', sans-serif;
}
</style>
