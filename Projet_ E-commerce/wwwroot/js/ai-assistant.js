document.addEventListener('DOMContentLoaded', function () {
    const chatToggle = document.getElementById('chat-toggle');
    const chatWindow = document.getElementById('chat-window');
    const closeChat = document.getElementById('close-chat');
    const chatContent = document.getElementById('chat-content');
    const chatInput = document.getElementById('chat-input');
    const sendMessage = document.getElementById('send-message');

    if (!chatToggle || !chatWindow) return;

    // Toggle chat window
    chatToggle.addEventListener('click', () => {
        chatWindow.classList.toggle('active');
        if (chatWindow.classList.contains('active')) {
            chatInput.focus();
        }
    });

    closeChat.addEventListener('click', () => {
        chatWindow.classList.remove('active');
    });

    // Handle sending message
    async function handleSend() {
        const text = chatInput.value.trim();
        if (!text) return;

        // Add user message to UI
        addMessage(text, 'user');
        chatInput.value = '';

        // Show typing indicator
        const typingId = showTyping();

        try {
            const response = await fetch('/api/AiAssistant/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text })
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error("Assistant Error:", errorText);
                throw new Error(errorText);
            }

            const data = await response.json();
            console.log("n8n Response Data:", data); // DEBUG: Show what n8n actually sent
            removeTyping(typingId);

            // Robust search for text content in n8n response
            function findText(obj) {
                if (!obj) return "";
                if (typeof obj === 'string') return obj;
                if (Array.isArray(obj)) return obj.length > 0 ? findText(obj[0]) : "";
                
                // Priority fields for AI agents
                const priorities = ['output', 'text', 'message', 'response', 'content'];
                for (const key of priorities) {
                    if (obj[key] && typeof obj[key] === 'string') return obj[key];
                    if (obj[key] && typeof obj[key] === 'object') {
                        const nested = findText(obj[key]);
                        if (nested) return nested;
                    }
                }

                // Fallback: look for any string property
                for (const key in obj) {
                    if (typeof obj[key] === 'string' && key !== 'sessionId' && key !== 'requestId') return obj[key];
                }
                return "";
            }

            let aiText = findText(data);
            
            if (!aiText || aiText.trim() === "") {
                aiText = "L'assistant n'a pas pu formuler de réponse textuelle. \n\n**Données reçues (Debug) :**\n```json\n" + JSON.stringify(data, null, 2).substring(0, 300) + "\n```\n\nVérifiez votre node 'Respond to Webhook'.";
            }

            addMessage(aiText, 'ai');
        } catch (error) {
            removeTyping(typingId);
            addMessage(`Oups! Une erreur s'est produite : ${error.message}`, 'ai');
        }
    }

    sendMessage.addEventListener('click', handleSend);
    chatInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') handleSend();
    });

    function addMessage(text, sender) {
        const msgDiv = document.createElement('div');
        msgDiv.className = `chat-message ${sender}-message mb-3`;
        msgDiv.innerHTML = `
            <div class="message-bubble shadow-sm">
                ${text}
            </div>
        `;
        chatContent.appendChild(msgDiv);
        chatContent.scrollTop = chatContent.scrollHeight;
    }

    function showTyping() {
        const id = 'typing-' + Date.now();
        const typingDiv = document.createElement('div');
        typingDiv.id = id;
        typingDiv.className = 'chat-message ai-message mb-3';
        typingDiv.innerHTML = `
            <div class="message-bubble typing-indicator shadow-sm">
                <span></span><span></span><span></span>
            </div>
        `;
        chatContent.appendChild(typingDiv);
        chatContent.scrollTop = chatContent.scrollHeight;
        return id;
    }

    function removeTyping(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }
});
