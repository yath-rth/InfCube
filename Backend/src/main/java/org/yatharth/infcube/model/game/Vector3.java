package org.yatharth.infcube.model.game;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Vector3 {
    public float x;
    public float y;
    public float z;

    public static Vector3 zero(){
        return new Vector3(0, 0, 0);
    }

    public static Vector3 forward(){
        return new Vector3(0,0,1);
    }

    public Vector3 add(Vector3 other) {
        return new Vector3(this.x + other.x, this.y + other.y, this.z + other.z);
    }

    public Vector3 subtract(Vector3 other) {
        return new Vector3(this.x - other.x, this.y - other.y, this.z - other.z);
    }

    public Vector3 scale(float scalar) {
        return new Vector3(this.x * scalar, this.y * scalar, this.z * scalar);
    }

    public Vector3 divide(float scalar) {
        return new Vector3(this.x / scalar, this.y / scalar, this.z / scalar);
    }

    public float dot(Vector3 other) {
        return this.x * other.x + this.y * other.y + this.z * other.z;
    }

    public Vector3 cross(Vector3 other) {
        return new Vector3(
                this.y * other.z - this.z * other.y,
                this.z * other.x - this.x * other.z,
                this.x * other.y - this.y * other.x
        );
    }

    public float magnitude() {
        return (float) Math.sqrt(x * x + y * y + z * z);
    }

    public Vector3 normalize() {
        float mag = magnitude();
        if (mag == 0) return new Vector3(0, 0, 0);
        return new Vector3(x / mag, y / mag, z / mag);
    }
}
