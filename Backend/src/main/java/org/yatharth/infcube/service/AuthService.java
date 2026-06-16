package org.yatharth.infcube.service;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class AuthService {

    // TODO: replace with Redis when setting up properly
    private final Map<String, String> sessions = new ConcurrentHashMap<>();

    public boolean hasActiveSession(String userId) {
        return sessions.containsKey(userId);
    }

    public String createSession(String userId) {
        String refreshToken = UUID.randomUUID().toString();
        sessions.put(userId, refreshToken);
        return refreshToken;
    }

    public boolean validateRefreshToken(String userId, String refreshToken) {
        return refreshToken.equals(sessions.get(userId));
    }

    public void deleteSession(String userId) {
        sessions.remove(userId);
    }
}