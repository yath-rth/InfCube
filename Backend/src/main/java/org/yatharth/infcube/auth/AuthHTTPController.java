package org.yatharth.infcube.auth;

import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.yatharth.infcube.data.UserRepository;
import org.yatharth.infcube.model.User;
import org.yatharth.infcube.model.UserCredentials;

import java.util.UUID;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class AuthHTTPController {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;

    @PostMapping("/login")
    public ResponseEntity<String> login(@RequestBody UserCredentials userCredentials) {
        if (!userRepository.existsByUsername(userCredentials.getUsername()))
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body("Username does not exist");
        User user = userRepository.findByUsername(userCredentials.getUsername());
        if (!passwordEncoder.matches(userCredentials.getPassword(), user.getPasswordHash()))
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).body("Password is incorrect");

        return ResponseEntity.ok("Password is incorrect");
    }

    @PostMapping("/register")
    public ResponseEntity<String> register(@RequestBody UserCredentials userCredentials) {
        if (userRepository.existsByUsername(userCredentials.getUsername()))
            return ResponseEntity.status(HttpStatus.CONFLICT).body("Username already exists");
        userRepository.save(new User(
                UUID.randomUUID().toString().substring(0, 4),
                userCredentials.getUsername(),
                passwordEncoder.encode(userCredentials.getPassword())
        ));

        return ResponseEntity.ok("Registration successful");
    }

}
