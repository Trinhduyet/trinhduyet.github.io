from pathlib import Path
import re, html, json
from collections import defaultdict, deque

OUT = Path('docs')
ASSET = OUT / 'assets' / 'diagrams'
ASSET.mkdir(parents=True, exist_ok=True)

PAPER='#f5f5f5'; INK='#2d3142'; MUTED='#4f5d75'; SOFT='#7a8399'; RULE='#d7d8dd'; ACCENT='#eb6c36'; ACCENT_TINT='#fff0e9'; LINK='#2e5aa8'; WHITE='#ffffff'
FONT_CSS = """@import url('https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&amp;family=Geist:wght@400;500;600&amp;family=Geist+Mono:wght@400;500;600&amp;display=swap');
text{font-family:'Geist',Inter,system-ui,sans-serif;fill:#2d3142}.mono{font-family:'Geist Mono',ui-monospace,monospace}.node-title{font-size:12px;font-weight:600}.sub{font-size:9px;fill:#4f5d75}.tag{font-size:7px;font-weight:500;letter-spacing:.16em;fill:#7a8399}.edge-label{font-size:8px;fill:#4f5d75}.callout{font-family:'Instrument Serif',Georgia,serif;font-style:italic;font-size:14px;fill:#4f5d75}svg{background:#f5f5f5}path,line,rect,polygon,circle{vector-effect:non-scaling-stroke}"""

def esc(s): return html.escape(str(s), quote=True)

def svg_open(w,h,title,desc=''):
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="100%" viewBox="0 0 {w} {h}" role="img" aria-labelledby="title desc">
<title id="title">{esc(title)}</title><desc id="desc">{esc(desc or title)}</desc>
<style>{FONT_CSS}</style>
<defs>
 <marker id="arr" markerWidth="7" markerHeight="7" refX="6" refY="3.5" orient="auto"><path d="M0,0 L7,3.5 L0,7 Z" fill="{MUTED}"/></marker>
 <marker id="arr-accent" markerWidth="7" markerHeight="7" refX="6" refY="3.5" orient="auto"><path d="M0,0 L7,3.5 L0,7 Z" fill="{ACCENT}"/></marker>
 <marker id="arr-link" markerWidth="7" markerHeight="7" refX="6" refY="3.5" orient="auto"><path d="M0,0 L7,3.5 L0,7 Z" fill="{LINK}"/></marker>
</defs>
<rect width="{w}" height="{h}" fill="{PAPER}"/>
'''

def svg_close(): return '</svg>\n'

def split_label(label):
    label = label.replace('<br/>','\n').replace('<br>','\n').replace('\\n','\n')
    label = re.sub(r'\s*·\s*', ' · ', label)
    raw=[x.strip() for x in label.split('\n') if x.strip()]
    lines=[]
    for line in raw:
        if len(line) <= 30:
            lines.append(line); continue
        words=line.split(); cur=''
        for word in words:
            trial=(cur+' '+word).strip()
            if cur and len(trial)>30:
                lines.append(cur); cur=word
            else: cur=trial
        if cur: lines.append(cur)
    if len(lines)>3: lines=lines[:2]+[' '.join(lines[2:])]
    return lines

def text_block(cx, cy, label, sub=None, anchor='middle'):
    lines=split_label(label) or ['']
    y0=cy-((len(lines)-1)*7); out=[]
    for i,line in enumerate(lines[:3]):
        cls='node-title' if i==0 else 'sub'
        out.append(f'<text x="{cx}" y="{y0+i*14}" text-anchor="{anchor}" class="{cls}">{esc(line)}</text>')
    if sub: out.append(f'<text x="{cx}" y="{y0+len(lines[:3])*14+2}" text-anchor="{anchor}" class="sub mono">{esc(sub)}</text>')
    return ''.join(out)

def node_kind(label):
    l=label.lower()
    if any(k in l for k in ['db','database','sql','redis','etcd','store','outbox','inbox','volume','index','registry']): return 'store'
    if any(k in l for k in ['user','client','request','source','learner','human','mobile','web','kubectl','ci / api client']): return 'input'
    if any(k in l for k in ['provider','external','os + hardware','container runtime']): return 'external'
    return 'backend'

def rect_node(x,y,w,h,label,kind='backend',focal=False,tag=None):
    if focal: fill,stroke=ACCENT_TINT,ACCENT
    elif kind=='store': fill,stroke='#e9eaed',MUTED
    elif kind=='input': fill,stroke='#e8edf2',SOFT
    elif kind=='external': fill,stroke='#efeff1','#9aa0ad'
    elif kind=='optional': fill,stroke='#f1f1f2','#a8acb5'
    else: fill,stroke=WHITE,INK
    dash=' stroke-dasharray="4 3"' if kind=='optional' else ''
    out=[f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="8" fill="{fill}" stroke="{stroke}" stroke-width="1.1"{dash}/>']
    if tag: out.append(f'<text x="{x+10}" y="{y+13}" class="tag mono">{esc(tag.upper())}</text>')
    out.append(text_block(x+w/2,y+h/2+4,label)); return ''.join(out)

def diamond_node(cx,cy,w,h,label,focal=False):
    fill=ACCENT_TINT if focal else WHITE; stroke=ACCENT if focal else INK
    pts=f'{cx},{cy-h/2} {cx+w/2},{cy} {cx},{cy+h/2} {cx-w/2},{cy}'
    return f'<polygon points="{pts}" fill="{fill}" stroke="{stroke}" stroke-width="1.1"/>'+text_block(cx,cy+4,label)

def edge_path(src,dst,direction='LR',dashed=False,label=None,accent=False,bidir=False):
    x1,y1,w1,h1=src; x2,y2,w2,h2=dst; stroke=ACCENT if accent else MUTED; marker='arr-accent' if accent else 'arr'; dash=' stroke-dasharray="4 3"' if dashed else ''
    if direction in ('LR','RL'):
        if x2>=x1: sx,sy=x1+w1,y1+h1/2; tx,ty=x2,y2+h2/2
        else: sx,sy=x1,y1+h1/2; tx,ty=x2+w2,y2+h2/2
        if abs(sy-ty)<2: d=f'M {sx} {sy} H {tx}'; lx=(sx+tx)/2; ly=sy-10
        else:
            mid=(sx+tx)/2; sgn=1 if ty>sy else -1
            d=f'M {sx} {sy} H {mid-8} Q {mid} {sy} {mid} {sy+8*sgn} V {ty-8*sgn} Q {mid} {ty} {mid+8 if tx>sx else mid-8} {ty} H {tx}'; lx=mid+8; ly=(sy+ty)/2-6
    else:
        if y2>=y1: sx,sy=x1+w1/2,y1+h1; tx,ty=x2+w2/2,y2
        else: sx,sy=x1+w1/2,y1; tx,ty=x2+w2/2,y2+h2
        if abs(sx-tx)<2: d=f'M {sx} {sy} V {ty}'; lx=sx+8; ly=(sy+ty)/2
        else:
            mid=(sy+ty)/2; sgn=1 if tx>sx else -1
            d=f'M {sx} {sy} V {mid-8} Q {sx} {mid} {sx+8*sgn} {mid} H {tx-8*sgn} Q {tx} {mid} {tx} {mid+8 if ty>sy else mid-8} V {ty}'; lx=(sx+tx)/2; ly=mid-6
    attrs=f'fill="none" stroke="{stroke}" stroke-width="1.1" marker-end="url(#{marker})"{dash}'
    if bidir: attrs+=f' marker-start="url(#{marker})"'
    out=[f'<path d="{d}" {attrs}/>']
    if label:
        tw=max(42,min(150,len(label)*5.4+14)); out.append(f'<rect x="{lx-tw/2}" y="{ly-10}" width="{tw}" height="15" rx="3" fill="{PAPER}"/>'); out.append(f'<text x="{lx}" y="{ly+1}" text-anchor="middle" class="edge-label mono">{esc(label)}</text>')
    return ''.join(out)

NODE_PAT=re.compile(r'([A-Za-z0-9_]+)\s*(\[\(.*?\)\]|\[.*?\]|\{.*?\})')
def clean_shape(shape):
    s=shape.strip(); decision=s.startswith('{'); store=s.startswith('[(')
    if decision: s=s[1:-1]
    elif store: s=s[2:-2]
    else: s=s[1:-1]
    return s.strip('"'),decision,store

def parse_flow(body):
    lines=[ln.strip() for ln in body.splitlines() if ln.strip()]; direction='LR'
    if lines and lines[0].startswith('flowchart'):
        p=lines.pop(0).split(); direction=p[1] if len(p)>1 else 'LR'
    nodes={}; order=[]; edges=[]
    for ln in lines:
        if ln.startswith(('classDef','class ')): continue
        toks=[]
        for m in NODE_PAT.finditer(ln):
            nid=m.group(1); label,dec,store=clean_shape(m.group(2))
            if nid not in nodes: nodes[nid]={'label':label,'decision':dec,'store':store}; order.append(nid)
            toks.append((m.start(),m.end(),nid))
        norm=ln
        for st,en,nid in reversed(toks): norm=norm[:st]+nid+norm[en:]
        if '-->' in norm or '-.->' in norm or '<-->' in norm or '.->' in norm:
            ids=[i for i in re.findall(r'(?<![A-Za-z0-9_])([A-Za-z][A-Za-z0-9_]*)(?![A-Za-z0-9_])',norm) if i in nodes]
            if len(ids)<2:
                rawids=re.findall(r'\b([A-Z][A-Z0-9_]*)\b',norm)
                for rid in rawids:
                    if rid not in nodes: nodes[rid]={'label':rid,'decision':False,'store':False}; order.append(rid)
                ids=[i for i in rawids if i in nodes]
            lm=re.search(r'\|([^|]+)\|',ln) or re.search(r'\.\s*"([^"]+)"\s*\.->',ln); label=lm.group(1) if lm else None
            for j in range(len(ids)-1): edges.append({'src':ids[j],'dst':ids[j+1],'label':label if j==0 else None,'dashed':'-.->' in ln or '.->' in ln,'bidir':'<-->' in ln})
    for ln in lines:
        if ln.startswith(('classDef','class ')) or not any(a in ln for a in ('-->','-.->','<-->','.->')): continue
        raw=re.sub(r'\|[^|]+\|','',ln); raw=re.sub(r'\.\s*"[^"]+"\s*\.->','-.->',raw)
        for m in list(NODE_PAT.finditer(raw))[::-1]: raw=raw[:m.start()]+m.group(1)+raw[m.end():]
        ids=[]
        for p in re.split(r'\s*(?:<-->|-->|-\.->|\.->)\s*',raw):
            m=re.match(r'([A-Za-z][A-Za-z0-9_]*)',p.strip())
            if m:
                rid=m.group(1)
                if rid not in nodes: nodes[rid]={'label':rid,'decision':False,'store':False}; order.append(rid)
                ids.append(rid)
        for j in range(len(ids)-1):
            if not any(e['src']==ids[j] and e['dst']==ids[j+1] for e in edges): edges.append({'src':ids[j],'dst':ids[j+1],'label':None,'dashed':'-.' in ln,'bidir':'<-->' in ln})
    return direction,nodes,order,edges

def layout_graph(direction,nodes,order,edges):
    indeg={n:0 for n in order}; out=defaultdict(list)
    for e in edges:
        if e['src'] in indeg and e['dst'] in indeg: indeg[e['dst']]+=1; out[e['src']].append(e['dst'])
    roots=[n for n in order if indeg[n]==0] or order[:1]; rank={}; q=deque()
    for r in roots: rank[r]=0; q.append(r)
    while q:
        u=q.popleft()
        for v in out[u]:
            if v not in rank: rank[v]=rank[u]+1; q.append(v)
    mr=max(rank.values(),default=0)
    for n in order:
        if n not in rank: mr+=1; rank[n]=mr
    groups=defaultdict(list)
    for n in order: groups[rank[n]].append(n)
    mc=max((len(v) for v in groups.values()),default=1); pos={}
    if direction in ('LR','RL'):
        gap=152; rg=92; m=32; nw=120; nh=62; w=m*2+(max(groups.keys(),default=0)+1)*gap; h=max(180,m*2+mc*rg)
        for r,a in groups.items():
            start=h/2-(len(a)-1)*rg/2-nh/2; x=m+r*gap
            if direction=='RL': x=w-m-nw-r*gap
            for i,n in enumerate(a): pos[n]=(x,start+i*rg,nw,nh)
    else:
        rg=100; cg=152; m=32; nw=120; nh=62; h=m*2+(max(groups.keys(),default=0)+1)*rg; w=max(220,m*2+mc*cg)
        for r,a in groups.items():
            start=w/2-(len(a)-1)*cg/2-nw/2; y=m+r*rg
            for i,n in enumerate(a): pos[n]=(start+i*cg,y,nw,nh)
    return int(w),int(h),pos

def render_flow(title,body,desc=''):
    direction,nodes,order,edges=parse_flow(body)
    if direction not in ('LR','RL') and len(order)>6:
        ind={n:0 for n in order}
        for e in edges:
            if e['dst'] in ind: ind[e['dst']]+=1
        if max(ind.values(),default=0)<=1: direction='LR'
    w,h,pos=layout_graph(direction,nodes,order,edges); deg={n:0 for n in order}
    for e in edges: deg[e['src']]=deg.get(e['src'],0)+1; deg[e['dst']]=deg.get(e['dst'],0)+1
    focal=max(order,key=lambda n:(deg.get(n,0),order.index(n))) if order else None; out=[svg_open(w,h,title,desc)]
    for e in edges:
        if e['src'] in pos and e['dst'] in pos: out.append(edge_path(pos[e['src']],pos[e['dst']],'LR' if direction in ('LR','RL') else 'TD',e['dashed'],e['label'],e['dst']==focal and len(edges)<9,e['bidir']))
    for n in order:
        nd=nodes[n]; x,y,nw,nh=pos[n]; kind='store' if nd['store'] else node_kind(nd['label']); foc=n==focal
        out.append(diamond_node(x+nw/2,y+nh/2,nw,nh,nd['label'],foc) if nd['decision'] else rect_node(x,y,nw,nh,nd['label'],kind,foc))
    out.append(svg_close()); return ''.join(out)

def render_sequence(title,body,desc=''):
    participants=[]; labels={}; msgs=[]
    for ln in body.splitlines()[1:]:
        ln=ln.strip(); m=re.match(r'participant\s+(\w+)\s+as\s+(.+)',ln)
        if m: pid,l=m.groups(); participants.append(pid); labels[pid]=l.strip(); continue
        m=re.match(r'(\w+)(--?>>|->>)(\w+):\s*(.+)',ln)
        if m: src,a,dst,l=m.groups(); msgs.append((src,dst,l,a.startswith('--')))
    w=max(640,80+len(participants)*180); h=110+len(msgs)*46; xs={p:70+i*(w-140)/(max(1,len(participants)-1)) for i,p in enumerate(participants)}; out=[svg_open(w,h,title,desc)]
    for i,p in enumerate(participants):
        x=xs[p]; out.append(rect_node(x-58,24,116,46,labels.get(p,p),focal=(i==1))); out.append(f'<line x1="{x}" y1="70" x2="{x}" y2="{h-24}" stroke="{RULE}" stroke-width="1" stroke-dasharray="4 4"/>')
    y=104
    for src,dst,label,dashed in msgs:
        x1,x2=xs[src],xs[dst]; stroke=LINK if ('tool' in label.lower() or 'http' in label.lower()) else MUTED; marker='arr-link' if stroke==LINK else 'arr'; dash=' stroke-dasharray="4 3"' if dashed else ''
        if src==dst: d=f'M {x1} {y} h 52 v 20 h -52'; out.append(f'<path d="{d}" fill="none" stroke="{stroke}" stroke-width="1.1" marker-end="url(#{marker})"{dash}/>'); lx=x1+26
        else: out.append(f'<line x1="{x1}" y1="{y}" x2="{x2}" y2="{y}" stroke="{stroke}" stroke-width="1.1" marker-end="url(#{marker})"{dash}/>'); lx=(x1+x2)/2
        tw=max(70,min(210,len(label)*5.2+18)); out.append(f'<rect x="{lx-tw/2}" y="{y-18}" width="{tw}" height="14" rx="3" fill="{PAPER}"/>'); out.append(f'<text x="{lx}" y="{y-8}" text-anchor="middle" class="edge-label mono">{esc(label)}</text>'); y+=46
    out.append(svg_close()); return ''.join(out)

def render_state_checkout(title,body,desc=''):
    w,h=980,500; out=[svg_open(w,h,title,desc)]; P={'Created':(52,70,130,58),'InventoryReserved':(226,70,150,58),'PendingPayment':(420,70,150,58),'Paid':(672,42,120,58),'PaymentFailed':(672,132,136,58),'PaymentUnknown':(420,204,150,58),'ShippingPending':(672,280,150,58),'Completed':(844,280,112,58),'Compensating':(672,388,150,58),'Failed':(844,388,112,58)}
    def e(a,b,label=None,dashed=False,accent=False): out.append(edge_path(P[a],P[b],'LR' if abs(P[a][1]-P[b][1])<80 else 'TD',dashed,label,accent))
    out.append(f'<circle cx="28" cy="99" r="6" fill="{INK}"/><line x1="34" y1="99" x2="52" y2="99" stroke="{MUTED}" marker-end="url(#arr)"/>'); e('Created','InventoryReserved'); e('InventoryReserved','PendingPayment'); e('PendingPayment','Paid','confirmed success',accent=True); e('PendingPayment','PaymentFailed','confirmed decline'); e('PendingPayment','PaymentUnknown','timeout / lost response',True); e('PaymentUnknown','Paid','reconcile: charged',True); e('PaymentUnknown','PaymentFailed','reconcile: no charge',True); e('Paid','ShippingPending'); e('ShippingPending','Completed','shipment ok',accent=True); e('ShippingPending','Compensating','shipment failed'); e('Compensating','Failed','refund + release')
    for n,p in P.items(): out.append(rect_node(*p,n,'optional' if n=='PaymentUnknown' else 'backend',n in ('PaymentUnknown','Completed')))
    out.append(f'<text x="52" y="250" class="callout">Unknown is not failure — reconcile before compensation.</text>'); out.append(svg_close()); return ''.join(out)

def render_prerequisites(title,body,desc=''):
    w,h=920,480; out=[svg_open(w,h,title,desc)]; layers=[('FOUNDATIONS','CS · Linux · Networking · C#/.NET',52,52,816,56,'input'),('BACKEND','HTTP · SQL · API Design · ASP.NET Core',52,128,816,56,'backend'),('PRODUCTION','Testing · Security · Performance · Observability',52,204,816,56,'backend'),('PLATFORM + DISTRIBUTED','Docker · Kubernetes · Transactions · Messaging · Outbox',52,280,816,56,'backend'),('ARCHITECTURE TRACK','Distributed Systems · Microservices · System Design · Architecture',52,356,816,56,'focal')]
    for i,(tag,label,x,y,nw,nh,kind) in enumerate(layers):
        out.append(rect_node(x,y,nw,nh,label,'backend' if kind=='focal' else kind,kind=='focal',tag));
        if i<len(layers)-1: out.append(f'<path d="M 460 {y+nh} V {layers[i+1][3]}" fill="none" stroke="{MUTED}" stroke-width="1.2" marker-end="url(#arr)"/>')
    out.append(f'<text x="460" y="450" text-anchor="middle" class="callout">AI Engineering branches after backend foundations; both tracks converge at system design.</text>'); out.append(svg_close()); return ''.join(out)

def render_roadmap_core(title,desc=''):
    w,h=1080,360; out=[svg_open(w,h,title,desc)]; nodes=[('Foundations','CS · Linux · Network'),('.NET Backend','C# · SQL · API · ASP.NET'),('Production','Test · Security · Perf'),('Platform','Docker · Cloud · K8s'),('Distributed','Failure · Messaging'),('Microservices','Boundaries · Data'),('System Design','NFR · Capacity'),('Architecture','Software + AI')]; xs=[28,154,294,426,562,696,832,962]; widths=[110,124,116,116,116,120,112,94]; y=112; P=[(xs[i],y,widths[i],70) for i in range(len(nodes))]
    for i in range(len(P)-1): out.append(edge_path(P[i],P[i+1],'LR',accent=(i==4)))
    for i,((a,b),p) in enumerate(zip(nodes,P)): out.append(rect_node(*p,a+'\n'+b,'input' if i==0 else 'backend',i in (5,7)))
    ai=(440,244,148,62); agents=(650,244,148,62); out.append(edge_path(P[1],ai,'TD',True,'AI lane')); out.append(edge_path(P[4],ai,'TD',True)); out.append(edge_path(ai,agents,'LR',accent=True)); out.append(edge_path(agents,P[6],'TD',True,'converges')); out.append(rect_node(*ai,'AI Engineering\nLLM · RAG · Evals',focal=True)); out.append(rect_node(*agents,'AI Agents\nCoding · Business')); out.append(svg_close()); return ''.join(out)

def render_coding_agent_loop(title,desc=''):
    w,h=1040,350; out=[svg_open(w,h,title,desc)]; P={'Task':(28,54,112,58),'Context':(174,54,126,58),'Plan':(334,54,104,58),'Edit':(472,54,104,58),'Test':(610,54,134,58),'Diagnose':(472,174,112,58),'Review':(610,174,126,58),'Security':(770,174,126,58),'PR':(930,174,82,58),'Human':(770,270,126,58)}
    def e(a,b,**kw): out.append(edge_path(P[a],P[b],'LR' if abs(P[a][1]-P[b][1])<30 else 'TD',**kw))
    e('Task','Context'); e('Context','Plan'); e('Plan','Edit'); e('Edit','Test',accent=True); e('Test','Diagnose',dashed=True,label='fail'); e('Diagnose','Edit',dashed=True,label='iterate'); e('Test','Review',label='pass'); e('Review','Security'); e('Security','PR'); e('PR','Human',accent=True)
    labels={'Task':'Task / Issue','Context':'Collect Context','Plan':'Plan','Edit':'Scoped Edit','Test':'Build + Tests','Diagnose':'Diagnose','Review':'Review Diff','Security':'Security Checks','PR':'Draft PR','Human':'Human Review'}
    for n,p in P.items(): out.append(rect_node(*p,labels[n],'input' if n=='Task' else 'backend',n in ('Test','Human')))
    out.append(f'<text x="28" y="316" class="callout">Executable evidence closes the loop; generation alone is not completion.</text>'); out.append(svg_close()); return ''.join(out)

def render_coding_agent_governed(title,desc=''):
    w,h=1040,330; out=[svg_open(w,h,title,desc)]; P={'Issue':(28,56,104,58),'Agent':(164,56,118,58),'Branch':(314,56,130,58),'Tests':(476,56,126,58),'Security':(634,56,126,58),'PR':(634,190,112,58),'Human':(778,190,126,58),'Merge':(936,190,76,58),'CICD':(778,270,126,46)}
    def e(a,b,**kw): out.append(edge_path(P[a],P[b],'LR' if abs(P[a][1]-P[b][1])<30 else 'TD',**kw))
    e('Issue','Agent'); e('Agent','Branch'); e('Branch','Tests'); e('Tests','Security',accent=True); e('Security','PR'); e('PR','Human'); e('Human','Merge',accent=True); e('Merge','CICD'); e('Human','Agent',dashed=True,label='changes')
    labels={'Issue':'Issue','Agent':'Coding Agent','Branch':'Feature Branch','Tests':'Build + Tests','Security':'Security Checks','PR':'Draft PR','Human':'Human Review','Merge':'Merge','CICD':'Normal CI/CD'}
    for n,p in P.items(): out.append(rect_node(*p,labels[n],'input' if n=='Issue' else 'backend',n in ('Security','Human')))
    out.append(svg_close()); return ''.join(out)

def asset_title(path,idx): return f"{path.stem.replace('-',' ').replace('_',' ').title()} — diagram {idx}"
def safe_name(rel,idx): return re.sub(r'[^a-zA-Z0-9_-]+','-',str(rel).replace('/','-').replace('.md','')).lower()+f'-{idx}.svg'
def img_md(rel,asset,alt): return f"![{alt}]({'../'*(len(rel.parts)-1)}assets/diagrams/{asset})"

catalog=[]
for md in sorted(OUT.rglob('*.md')):
    rel=md.relative_to(OUT); text=md.read_text(encoding='utf-8'); blocks=list(re.finditer(r'```mermaid\n(.*?)\n```',text,re.S))
    if not blocks: continue
    reps=[]
    for idx,m in enumerate(blocks,1):
        body=m.group(1).strip(); title=asset_title(rel,idx); alt=f'Sơ đồ {title}'
        if str(rel) in ('index.md','00-roadmap/master-roadmap.md') and idx==1:
            asset='roadmap-core-and-ai.svg'
            if not (ASSET/asset).exists(): (ASSET/asset).write_text(render_roadmap_core('AI-enabled Software Architect roadmap'),encoding='utf-8')
            repl=img_md(rel,asset,'Roadmap từ foundations đến Software / AI Architecture')
        elif str(rel)=='00-roadmap/prerequisites.md' and idx==1:
            asset='prerequisites-layer-stack.svg'; (ASSET/asset).write_text(render_prerequisites(title,body),encoding='utf-8'); repl=img_md(rel,asset,'Dependency layers từ foundations đến architecture')
        elif str(rel)=='21-ai-coding-agents/README.md' and idx==1:
            asset=safe_name(rel,idx); (ASSET/asset).write_text(render_coding_agent_loop(title),encoding='utf-8'); repl=img_md(rel,asset,'Vòng lặp AI Coding Agent: context, edit, executable evidence và human review')
        elif str(rel)=='21-ai-coding-agents/README.md' and idx==2:
            asset=safe_name(rel,idx); (ASSET/asset).write_text(render_coding_agent_governed(title),encoding='utf-8'); repl=img_md(rel,asset,'Governed AI Coding Agent workflow: branch, tests, security, PR và CI/CD')
        elif body.startswith('stateDiagram-v2'):
            asset=safe_name(rel,idx); (ASSET/asset).write_text(render_state_checkout(title,body),encoding='utf-8'); repl=img_md(rel,asset,'State machine checkout: unknown payment, reconciliation và compensation')
        elif body.startswith('sequenceDiagram'):
            asset=safe_name(rel,idx); (ASSET/asset).write_text(render_sequence(title,body),encoding='utf-8'); repl=img_md(rel,asset,alt)
        else:
            asset=safe_name(rel,idx); (ASSET/asset).write_text(render_flow(title,body),encoding='utf-8'); repl=img_md(rel,asset,alt)
        reps.append((m.start(),m.end(),repl)); catalog.append((str(rel),idx,asset))
    for st,en,repl in reversed(reps): text=text[:st]+repl+text[en:]
    md.write_text(text,encoding='utf-8')

(ASSET/'README.md').write_text('''# Diagram Design assets\n\nThese SVGs replace Mermaid auto-layout diagrams and follow the editorial rules from [`cathrynlavery/diagram-design`](https://github.com/cathrynlavery/diagram-design): low density, one focal accent, no shadows, restrained corner radius, semantic node treatments and orthogonal connectors.\n\nDefault skin used here: paper `#f5f5f5`, ink `#2d3142`, muted `#4f5d75`, accent `#eb6c36`, link `#2e5aa8`.\n\nGenerated/updated: 2026-08-13.\n''',encoding='utf-8')
(ASSET/'catalog.json').write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding='utf-8')
mk=Path('mkdocs.yml'); mt=mk.read_text(); mt=re.sub(r"\n  - pymdownx\.superfences:\n      custom_fences:\n        - name: mermaid\n          class: mermaid\n          format: !!python/name:pymdownx\.superfences\.fence_code_format\n","\n  - pymdownx.superfences\n",mt); mk.write_text(mt)
remaining=[str(p.relative_to(OUT)) for p in OUT.rglob('*.md') if '```mermaid' in p.read_text()]
print('Diagram Design assets:',len(list(ASSET.glob('*.svg'))),'remaining Mermaid:',remaining)
if remaining: raise SystemExit(1)
