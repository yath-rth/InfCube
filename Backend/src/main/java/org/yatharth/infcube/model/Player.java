package org.yatharth.infcube.model;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Player {
    public Vector3 position;
    public Vector3 direction;
    public float speed;
}
