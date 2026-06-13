package org.yatharth.infcube.auth;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.yatharth.infcube.model.UserCredentials;

@RestController
@RequestMapping("/auth")
public class AuthHTTPController {

    @PostMapping("/login")
    public String login() {
        return "Login successful";
    }

    @PostMapping("/register")
    public String register(@RequestBody UserCredentials userCredentials) {
        return "Registration successful";
    }

}
