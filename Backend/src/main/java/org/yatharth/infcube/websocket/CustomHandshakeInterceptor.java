package org.yatharth.infcube.auth;

import lombok.RequiredArgsConstructor;
import org.springframework.http.server.ServerHttpRequest;
import org.springframework.http.server.ServerHttpResponse;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketHandler;
import org.springframework.web.socket.server.HandshakeInterceptor;
import org.yatharth.infcube.data.UserRepository;
import org.yatharth.infcube.model.auth.User;
import org.yatharth.infcube.service.AuthService;
import org.yatharth.infcube.service.JwtService;

import java.util.Map;

@Component
@RequiredArgsConstructor
public class CustomHandshakeInterceptor implements HandshakeInterceptor {

    private final JwtService jwtService;
    private final UserRepository userRepository;
    private final AuthService authService;

    @Override
    public boolean beforeHandshake(
            ServerHttpRequest request,
            ServerHttpResponse response,
            WebSocketHandler wsHandler,
            Map<String, Object> attributes
    ) {

        return true;

//        try {
//            String query = request.getURI().getQuery();
//
//            if (query == null || !query.startsWith("token=")) {
//                return false;
//            }
//
//            String token = query.substring("token=".length());
//
//            String username = jwtService.extractUsername(token);
//
//            User user = userRepository.findByUsername(username);
//
//            if (user == null) {
//                return false;
//            }
//
//            if (!authService.hasActiveSession(user.getUserId())) {
//                return false;
//            }
//
//            attributes.put("userId", user.getUserId());
//            attributes.put("username", user.getUsername());
//
//            return true;
//
//        } catch (Exception e) {
//            return false;
//        }
    }

    @Override
    public void afterHandshake(
            ServerHttpRequest request,
            ServerHttpResponse response,
            WebSocketHandler wsHandler,
            Exception exception
    ) {
    }
}