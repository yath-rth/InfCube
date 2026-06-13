package org.yatharth.infcube.model.payloads;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.yatharth.infcube.model.PathInfo;

import java.util.List;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class NewMapPayload {
    public List<PathInfo> extension;
}
