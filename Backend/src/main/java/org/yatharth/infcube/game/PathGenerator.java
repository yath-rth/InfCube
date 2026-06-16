package org.yatharth.infcube.game;

import org.springframework.stereotype.Component;
import org.yatharth.infcube.model.game.PathInfo;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

@Component
public class PathGenerator {
    public List<PathInfo> generatePath(int seed) {
        List<PathInfo> path = new ArrayList<>();
        Random rng = new Random(seed);
        int prevSide = rng.nextInt(2);

        for (int i = 0; i < 1000; i++) {
            int count = rng.nextInt(GameConstants.MAX_LENGTH - GameConstants.MIN_LENGTH + 1) + GameConstants.MIN_LENGTH;
            int side = prevSide ^ 1;
            prevSide = side;
            path.add(new PathInfo(side, count));
        }

        return path;
    }
}
