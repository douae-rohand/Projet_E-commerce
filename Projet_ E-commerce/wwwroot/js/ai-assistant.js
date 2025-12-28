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
            removeTyping(typingId);

            // Handle n8n response format
            // Based on n8n "AI Agent" node, usually it returns an array [ { output: "text" } ] 
            // or if via Webhook it might be the direct object.
            let aiText = "";
            
            // Aggressive search for content in n8n response
            if (Array.isArray(data) && data.length > 0) {
                const first = data[0];
                aiText = first.output || first.text || first.message || (typeof first === 'string' ? first : "");
            } else if (typeof data === 'object' && data !== null) {
                aiText = data.output || data.text || data.message || "";
                // If it's the raw webhook response object (which we saw in logs), don't show it
                if (data.headers && data.body) aiText = ""; 
            } else if (typeof data === 'string') {
                aiText = data;
            }

            if (!aiText || aiText.trim() === "") {
                aiText = "L'assistant réfléchit... ou n'a pas pu formuler de réponse. Vérifiez la configuration n8n.";
            }

            addMessage(aiText, 'ai');
        } catch (error) {
            removeTyping(typingId);
            addMessage("Oups! Une erreur s'est produite. Réessayez plus tard.", 'ai');
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
