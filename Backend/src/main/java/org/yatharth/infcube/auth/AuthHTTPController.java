package org.yatharth.infcube.auth;

import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;
import org.yatharth.infcube.data.UserRepository;
import org.yatharth.infcube.model.auth.AuthResponse;
import org.yatharth.infcube.model.auth.RefreshRequest;
import org.yatharth.infcube.model.auth.User;
import org.yatharth.infcube.model.auth.UserCredentials;
import org.yatharth.infcube.service.AuthService;
import org.yatharth.infcube.service.JwtService;

import java.util.UUID;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class AuthHTTPController {

    private final UserRepository userRepository;
    private final JwtService jwtService;
    private final PasswordEncoder passwordEncoder;
    private final AuthService authService;

    @PostMapping("/register")
    public ResponseEntity<?> register(@RequestBody UserCredentials credentials) {
        User user = userRepository.findByUsername(credentials.getUsername()).orElse(null);
        if (user != null)
            return ResponseEntity.badRequest().body("Username already exists");

        user = userRepository.save(User.builder()
                .userId(UUID.randomUUID().toString())
                .username(credentials.getUsername())
                .passwordHash(passwordEncoder.encode(credentials.getPassword()))
                .build());

        String token = jwtService.generateToken(user.getUsername());
        String refreshToken = authService.createSession(user.getUserId());

        return ResponseEntity.ok(new AuthResponse(token, refreshToken));
    }

    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody UserCredentials credentials) {
        User user = userRepository.findByUsername(credentials.getUsername()).orElse(null);
        if (user == null)
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body("User not found");
        if (!passwordEncoder.matches(credentials.getPassword(), user.getPasswordHash()))
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).body("Invalid password");
        if (authService.hasActiveSession(user.getUserId()))
            return ResponseEntity.status(HttpStatus.CONFLICT).body("Already logged in");

        String token = jwtService.generateToken(user.getUsername());
        String refreshToken = authService.createSession(user.getUserId());  // unique per user
        return ResponseEntity.ok(new AuthResponse(token, refreshToken));
    }

    @PostMapping("/refresh")
    public ResponseEntity<?> refresh(@RequestBody RefreshRequest request) {
        String username = jwtService.extractUsername(request.getToken());
        User user = userRepository.findByUsername(username).orElse(null);
        if (user == null)
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body("User not found");

        if (!authService.validateRefreshToken(user.getUserId(), request.getRefreshToken()))
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).body("Invalid refresh token");

        String newToken = jwtService.generateToken(username);
        return ResponseEntity.ok(new AuthResponse(newToken, request.getRefreshToken()));
    }

    @PostMapping("/logout")
    public ResponseEntity<?> logout(@RequestHeader("Authorization") String authHeader) {
        String token = authHeader.substring(7);
        String username = jwtService.extractUsername(token);

        User user = userRepository.findByUsername(username).orElse(null);
        if (user == null)
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body("User not found");

        authService.deleteSession(user.getUserId());
        return ResponseEntity.ok("Logged out");
    }

}
