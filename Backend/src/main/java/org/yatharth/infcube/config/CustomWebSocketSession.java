package org.yatharth.infcube.config;

import lombok.RequiredArgsConstructor;
import lombok.experimental.Delegate;
import org.springframework.web.socket.*;
import java.io.IOException;

@RequiredArgsConstructor
public class CustomWebSocketSession implements WebSocketSession {

    @Delegate
    private final WebSocketSession delegate;

    @Override
    public void sendMessage(WebSocketMessage<?> message) throws IOException {
        synchronized (delegate) {
            delegate.sendMessage(message);
        }
    }
}
