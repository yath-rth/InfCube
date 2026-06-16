package org.yatharth.infcube.data;

import org.springframework.stereotype.Repository;
import org.yatharth.infcube.model.auth.User;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Repository
public class UserRepository {

    public final Map<String, User> users = new ConcurrentHashMap<>();

    public User save(User user) {
        return users.put(user.getUsername(), user);
    }

    public User findByUsername(String username) {
        return users.get(username);
    }

    public boolean existsByUsername(String username) {
        return users.containsKey(username);
    }

    public void remove(User user) {
        users.remove(user.getUsername());
    }
}
